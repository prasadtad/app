using System;
using Amazon.DynamoDBv2;
using System.Threading.Tasks;
using RecipeShelf.Common.Models;
using Amazon.Runtime;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace RecipeShelf.Common.Proxies
{
    public sealed class DynamoDbProxy : INoSqlDbProxy, IDisposable
    {
        private readonly ILogger<DynamoDbProxy> _logger;

        private AmazonDynamoDBClient _client;

        public DynamoDbProxy(ILogger<DynamoDbProxy> logger, IOptions<CommonSettings> optionsAccessor)
        {
            _logger = logger;
            _client = optionsAccessor.Value.UseLocalDynamoDB ? new AmazonDynamoDBClient(new BasicAWSCredentials("Local", "Local"), new AmazonDynamoDBConfig { ServiceURL = "http://localhost:24413" })
                                                             : new AmazonDynamoDBClient();
        }

        public async Task<bool> CanConnectAsync()
        {
            _logger.LogDebug("Checking if Recipes and Ingredients tables exist in DynamoDB");

            var response = await _client.ListTablesAsync();
            return response.TableNames != null && response.TableNames.Contains("Recipes") && response.TableNames.Contains("Ingredients");
        }

        public async Task DeleteIngredientAsync(string id)
        {
            _logger.LogDebug("Deleting Ingredient {Id} from DynamoDB", id);

            var ingredientTable = Table.LoadTable(_client, "Ingredients");
            await ingredientTable.DeleteItemAsync(new Primitive(id));
        }

        public async Task DeleteRecipeAsync(string id)
        {
            _logger.LogDebug("Deleting Recipe {Id} from DynamoDB", id);

            var recipeTable = Table.LoadTable(_client, "Recipes");
            await recipeTable.DeleteItemAsync(new Primitive(id));
        }

        public async Task<Recipe> GetRecipeAsync(string id)
        {
            _logger.LogDebug("Getting Recipe {Id} from DynamoDB", id);

            var recipeTable = Table.LoadTable(_client, "Recipes");
            var doc = await recipeTable.GetItemAsync(new Primitive(id));

            return new Recipe(id,
                                    doc["lastModified"].AsDateTime(),
                                    doc["names"].AsArrayOfString(),
                                    doc["description"].AsString(),
                                    doc.ContainsKey("steps") ? FromDynamoDBList(doc["steps"].AsDynamoDBList()) : null,
                                    doc["totalTimeInMinutes"].AsInt(),
                                    doc.ContainsKey("servings") ? doc["servings"].AsString() : null,
                                    doc["approved"].AsBoolean(),
                                    (SpiceLevel)Enum.Parse(typeof(SpiceLevel), doc["spiceLevel"].AsString()),
                                    doc.ContainsKey("region") ? doc["region"].AsString() : null,
                                    doc["chefId"].AsString(),
                                    doc.ContainsKey("imageId") ? doc["imageId"].AsString() : null,
                                    doc.ContainsKey("ingredients") ? FromDynamoDBList(doc["ingredients"].AsDynamoDBList()) : null,
                                    doc["cuisine"].AsString(),
                                    doc.ContainsKey("ingredientIds") ? doc["ingredientIds"].AsArrayOfString() : null,
                                    doc["overnightPreparation"].AsBoolean(),
                                    doc.ContainsKey("accompanimentIds") ? doc["accompanimentIds"].AsArrayOfString() : null,
                                    doc.ContainsKey("collections") ? doc["collections"].AsArrayOfString() : null);
        }

        public async Task PutRecipeAsync(Recipe recipe)
        {
            _logger.LogDebug("Putting Recipe {Id} into DynamoDB", recipe.Id);

            var recipeTable = Table.LoadTable(_client, "Recipes");

            var doc = new Document();
            if (recipe.AccompanimentIds != null && recipe.AccompanimentIds.Length > 0)
                doc["accompanimentIds"] = recipe.AccompanimentIds;
            doc["approved"] = new DynamoDBBool(recipe.Approved);
            doc["chefId"] = recipe.ChefId;
            if (recipe.Collections != null && recipe.Collections.Length > 0)
                doc["collections"] = recipe.Collections;
            if (!string.IsNullOrEmpty(recipe.Cuisine))
                doc["cuisine"] = recipe.Cuisine;
            if (!string.IsNullOrEmpty(recipe.Description))
                doc["description"] = recipe.Description;
            doc["id"] = (string)recipe.Id;
            if (!string.IsNullOrEmpty(recipe.ImageId))
                doc["imageId"] = recipe.ImageId;
            if (recipe.IngredientIds != null && recipe.IngredientIds.Length > 0)
                doc["ingredientIds"] = recipe.IngredientIds;
            if (recipe.Ingredients != null && recipe.Ingredients.Length > 0)
                doc["ingredients"] = ToDynamoDBList(recipe.Ingredients);
            doc["lastModified"] = recipe.LastModified;
            doc["names"] = recipe.Names;
            doc["overnightPreparation"] = new DynamoDBBool(recipe.OvernightPreparation);
            if (!string.IsNullOrEmpty(recipe.Region))
                doc["region"] = recipe.Region;
            if (!string.IsNullOrEmpty(recipe.Servings))
                doc["servings"] = recipe.Servings;
            doc["spiceLevel"] = recipe.SpiceLevel.ToString();
            if (recipe.Steps != null && recipe.Steps.Length > 0)
                doc["steps"] = ToDynamoDBList(recipe.Steps);
            doc["totalTimeInMinutes"] = recipe.TotalTimeInMinutes;

            await recipeTable.PutItemAsync(doc);
        }

        public async Task<Ingredient> GetIngredientAsync(string id)
        {
            _logger.LogDebug("Getting Ingredient {Id} from DynamoDB", id);

            var ingredientTable = Table.LoadTable(_client, "Ingredients");
            var doc = await ingredientTable.GetItemAsync(new Primitive(id));
            return new Ingredient(id,
                        doc["lastModified"].AsDateTime(),
                        doc["names"].AsArrayOfString(),
                        doc.ContainsKey("description") ? doc["description"].AsString() : null,
                        doc.ContainsKey("category") ? doc["category"].AsString() : null,
                        doc["vegan"].AsBoolean());
        }

        public async Task PutIngredientAsync(Ingredient ingredient)
        {
            _logger.LogDebug("Putting Ingredient {Id} into DynamoDB", ingredient.Id);

            var ingredientTable = Table.LoadTable(_client, "Ingredients");

            var doc = new Document();
            if (!string.IsNullOrEmpty(ingredient.Category))
                doc["category"] = ingredient.Category;
            doc["id"] = (string)ingredient.Id;
            doc["lastModified"] = ingredient.LastModified;
            doc["names"] = ingredient.Names;
            if (!string.IsNullOrEmpty(ingredient.Description))
                doc["description"] = ingredient.Description;
            doc["vegan"] = new DynamoDBBool(ingredient.Vegan);

            await ingredientTable.PutItemAsync(doc);
        }

        private RecipeItem[] FromDynamoDBList(DynamoDBList list)
        {
            var recipeItems = new List<RecipeItem>();
            foreach (var entry in list.Entries)
            {
                var doc = entry.AsDocument();
                recipeItems.Add(new RecipeItem(doc["text"].AsString(),
                                               (Decorator)Enum.Parse(typeof(Decorator), doc["decorator"].AsString())));
            }
            return recipeItems.ToArray();
        }

        private DynamoDBList ToDynamoDBList(RecipeItem[] recipeItems)
        {
            var list = new DynamoDBList();
            foreach (var ri in recipeItems)
            {
                var item = new Document();
                item["decorator"] = ri.Decorator.ToString();
                item["text"] = ri.Text;
                list.Add(item);
            }
            return list;
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }
        }
    }
}
