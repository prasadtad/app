using System;
using Amazon.DynamoDBv2;
using System.Threading.Tasks;
using RecipeShelf.Common.Models;
using Amazon.Runtime;
using Amazon.DynamoDBv2.DocumentModel;
using RecipeShelf.Common;

namespace RecipeShelf.NoSql
{
    public sealed class DynamoDbProxy : INoSqlDbProxy, IDisposable
    {
        private readonly Logger<DynamoDbProxy> _logger = new Logger<DynamoDbProxy>();

        private AmazonDynamoDBClient _client;

        public DynamoDbProxy()
        {
            _client = Settings.UseLocalDynamoDB ? new AmazonDynamoDBClient(new BasicAWSCredentials("Local", "Local"), new AmazonDynamoDBConfig { ServiceURL = "http://localhost:8000" }) : new AmazonDynamoDBClient();
        }

        public async Task PutRecipeAsync(Recipe recipe)
        {
            _logger.Debug("PutRecipe", $"Putting {recipe.Id.Value} into DynamoDB");

            var recipeTable = Table.LoadTable(_client, Constants.RECIPE_TABLE_NAME);

            var doc = new Document();
            if (recipe.AccompanimentIds != null && recipe.AccompanimentIds.Length > 0)
                doc["accompanimentIds"] = recipe.AccompanimentIds.ToStrings();
            doc["approved"] = new DynamoDBBool(recipe.Approved);
            doc["chefId"] = recipe.ChefId;
            if (recipe.Collections != null && recipe.Collections.Length > 0)
                doc["collections"] = recipe.Collections;
            if (!string.IsNullOrEmpty(recipe.Cuisine))
                doc["cuisine"] = recipe.Cuisine;
            if (!string.IsNullOrEmpty(recipe.Description))
                doc["description"] = recipe.Description;
            doc["id"] = recipe.Id.Value;
            if (!string.IsNullOrEmpty(recipe.ImageId))
                doc["imageId"] = recipe.ImageId;
            if (recipe.IngredientIds != null && recipe.IngredientIds.Length > 0)
                doc["ingredientIds"] = recipe.IngredientIds.ToStrings();
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

        public async Task PutIngredientAsync(Ingredient ingredient)
        {
           _logger.Debug("PutIngredient", $"Putting {ingredient.Id.Value} into DynamoDB");

            var ingredientTable = Table.LoadTable(_client, Constants.INGREDIENT_TABLE_NAME);

            var doc = new Document();
            if (!string.IsNullOrEmpty(ingredient.Category))
                doc["category"] = ingredient.Category;
            doc["id"] = ingredient.Id.Value;
            doc["lastModified"] = ingredient.LastModified;
            doc["names"] = ingredient.Names;
            doc["vegan"] = new DynamoDBBool(ingredient.Vegan);

            await ingredientTable.PutItemAsync(doc);
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
