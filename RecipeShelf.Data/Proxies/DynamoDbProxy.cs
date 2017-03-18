using System;
using Amazon.DynamoDBv2;
using System.Threading.Tasks;
using RecipeShelf.Common.Models;
using Amazon.Runtime;
using Amazon.DynamoDBv2.DocumentModel;
using RecipeShelf.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Collections.Generic;

namespace RecipeShelf.Data.Proxies
{
    public sealed class DynamoDbProxy : INoSqlDbProxy, IDisposable
    {
        private readonly ILogger<DynamoDbProxy> _logger;

        private readonly DataSettings _settings;

        private AmazonDynamoDBClient _client;

        public DynamoDbProxy(ILogger<DynamoDbProxy> logger, IOptions<DataSettings> optionsAccessor)
        {
            _logger = logger;
            _settings = optionsAccessor.Value;
            _client = _settings.UseLocalDynamoDB ? new AmazonDynamoDBClient(new BasicAWSCredentials("Local", "Local"), new AmazonDynamoDBConfig { ServiceURL = "http://localhost:8000" }) : new AmazonDynamoDBClient();
        }

        public async Task<bool> CanConnectAsync()
        {
            _logger.LogDebug("Checking if Recipes and Ingredients tables exist in DynamoDB");
            var response = await _client.ListTablesAsync();
            return response.TableNames != null && response.TableNames.Contains("Recipes") && response.TableNames.Contains("Ingredients");
        }

        public async Task<Recipe> GetRecipeAsync(string id)
        {
            _logger.LogDebug("Getting Recipe {Id} from DynamoDB", id);

            var recipeTable = Table.LoadTable(_client, "Recipes");
            var doc = await recipeTable.GetItemAsync(new Primitive(id));

            var recipe = new Recipe { Id = id };
            recipe.AccompanimentIds = doc.ContainsKey("accompanimentIds") ? doc["accompanimentIds"].AsArrayOfString() : null;
            recipe.Approved = doc["approved"].AsBoolean();
            recipe.ChefId = doc["chefId"].AsString();
            recipe.Collections = doc.ContainsKey("collections") ? doc["collections"].AsArrayOfString() : null;
            recipe.Cuisine = doc["cuisine"].AsString();
            recipe.Description = doc["description"].AsString();
            recipe.ImageId = doc.ContainsKey("imageId") ? doc["imageId"].AsString() : null;
            recipe.IngredientIds = doc.ContainsKey("ingredientIds") ? doc["ingredientIds"].AsArrayOfString() : null;
            recipe.Ingredients = doc.ContainsKey("ingredients") ? FromDynamoDBList(doc["ingredients"].AsDynamoDBList()) : null;
            recipe.LastModified = doc["lastModified"].AsDateTime();
            recipe.Names = doc["names"].AsArrayOfString();
            recipe.OvernightPreparation = doc["overnightPreparation"].AsBoolean();
            recipe.Region = doc.ContainsKey("region") ? doc["region"].AsString() : null;
            recipe.Servings = doc.ContainsKey("servings") ? doc["servings"].AsString() : null;
            recipe.SpiceLevel = (SpiceLevel)Enum.Parse(typeof(SpiceLevel), doc["spiceLevel"].AsString());
            recipe.Steps = doc.ContainsKey("steps") ? FromDynamoDBList(doc["steps"].AsDynamoDBList()) : null;
            recipe.TotalTimeInMinutes = doc["totalTimeInMinutes"].AsInt();

            return recipe;
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
            var ingredient = new Ingredient { Id = id };
            ingredient.LastModified = doc["lastModified"].AsDateTime();
            ingredient.Names = doc["names"].AsArrayOfString();
            ingredient.Description = doc.ContainsKey("description") ? doc["description"].AsString() : null;
            ingredient.Category = doc.ContainsKey("category") ? doc["category"].AsString() : null;
            ingredient.Vegan = doc["vegan"].AsBoolean();
            return ingredient;
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
                recipeItems.Add(new RecipeItem
                {
                    Text = doc["text"].AsString(),
                    Decorator = (Decorator)Enum.Parse(typeof(Decorator), doc["decorator"].AsString())
                });
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
