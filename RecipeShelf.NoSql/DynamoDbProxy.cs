using System;
using Amazon.DynamoDBv2;
using System.Threading.Tasks;
using RecipeShelf.Common.Models;
using Amazon.Runtime;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RecipeShelf.Common;

namespace RecipeShelf.NoSql
{
    public sealed class DynamoDbProxy : INoSqlDbProxy, IDisposable
    {
        private readonly static Regex _phrasesRegex = new Regex("[a-z0-9]+", RegexOptions.Compiled);

        private readonly Logger<DynamoDbProxy> _logger = new Logger<DynamoDbProxy>();

        private AmazonDynamoDBClient _client;

        public DynamoDbProxy()
        {
            _client = Settings.UseLocalDynamoDB ? new AmazonDynamoDBClient(new BasicAWSCredentials("Local", "Local"), new AmazonDynamoDBConfig { ServiceURL = "http://localhost:8000" }) : new AmazonDynamoDBClient();
        }

        public async Task PutRecipeAsync(Recipe recipe)
        {
            var oldNames = await GetNamesAsync(Constants.RECIPE_TABLE_NAME, recipe.Id.Value);

            var batch = CreatePhrasesBatch(Constants.RECIPE_TABLE_NAME, recipe.Id.Value, recipe.Names, oldNames);

            _logger.Debug("PutRecipe", $"Putting {recipe.Id.Value} into DynamoDB");

            var recipeTable = Table.LoadTable(_client, Constants.RECIPE_TABLE_NAME);

            var doc = new Document();
            if (recipe.AccompanimentIds != null && recipe.AccompanimentIds.Length > 0)
                doc["accompanimentIds"] = recipe.AccompanimentIds.ToStrings();
            doc["approved"] = recipe.Approved;
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
            doc["overnightPreparation"] = recipe.OvernightPreparation;
            if (!string.IsNullOrEmpty(recipe.Region))
                doc["region"] = recipe.Region;
            if (!string.IsNullOrEmpty(recipe.Servings))
                doc["servings"] = recipe.Servings;
            doc["spiceLevel"] = recipe.SpiceLevel.ToString();
            if (recipe.Steps != null && recipe.Steps.Length > 0)
                doc["steps"] = ToDynamoDBList(recipe.Steps);
            doc["totalTimeInMinutes"] = recipe.TotalTimeInMinutes;

            var recipeBatch = recipeTable.CreateBatchWrite();
            recipeBatch.AddDocumentToPut(doc);

            await batch.Combine(recipeBatch).ExecuteAsync();
        }

        public async Task PutIngredientAsync(Ingredient ingredient)
        {
            var oldNames = await GetNamesAsync(Constants.INGREDIENT_TABLE_NAME, ingredient.Id.Value);

            var batch = CreatePhrasesBatch(Constants.INGREDIENT_TABLE_NAME, ingredient.Id.Value, ingredient.Names, oldNames);

            _logger.Debug("PutIngredient", $"Putting {ingredient.Id.Value} into DynamoDB");

            var ingredientTable = Table.LoadTable(_client, Constants.INGREDIENT_TABLE_NAME);

            var doc = new Document();
            if (!string.IsNullOrEmpty(ingredient.Category))
                doc["category"] = ingredient.Category;
            doc["id"] = ingredient.Id.Value;
            doc["lastModified"] = ingredient.LastModified;
            doc["names"] = ingredient.Names;
            doc["vegan"] = ingredient.Vegan;

            var ingredientBatch = ingredientTable.CreateBatchWrite();
            ingredientBatch.AddDocumentToPut(doc);

            await batch.Combine(ingredientBatch).ExecuteAsync();
        }

        private async Task<List<string>> GetNamesAsync(string table, string id)
        {
            _logger.Debug("GetNames", $"Getting {table} {id} old names from DynamoDB");
            var key = new Dictionary<string, AttributeValue>();
            key.Add("id", new AttributeValue(id));
            var response = await _client.GetItemAsync(new GetItemRequest { TableName = table, Key = key, ExpressionAttributeNames = new Dictionary<string, string> { ["#names"] = "names" }, ProjectionExpression = "#names" });
            if (response.IsItemSet && response.Item.ContainsKey("names"))
                return response.Item["names"].SS;
            return null;
        }

        private DocumentBatchWrite CreatePhrasesBatch(string sourceTable, string id, string[] names, List<string> oldNames)
        {
            var phraseTable = Table.LoadTable(_client, Constants.PHRASE_TABLE_NAME);

            var batch = phraseTable.CreateBatchWrite();

            var allPhrases = new HashSet<string>();
            var newPhraseKeys = GetPhraseKeys(allPhrases, sourceTable, id, names);

            if (oldNames != null)
            {
                _logger.Debug("PutPhrases", $"Deleting {sourceTable} {id} [{string.Join(",", oldNames)}] from DynamoDB");
                foreach (var key in GetPhraseKeys(allPhrases, sourceTable, id, oldNames))
                    batch.AddKeyToDelete(key);
            }
            _logger.Debug("PutPhrases", $"Adding {sourceTable} {id} [{string.Join(",", names)}] into DynamoDB");
            foreach (var key in newPhraseKeys)
                batch.AddDocumentToPut(new Document(key));

            return batch;
        }

        private List<Dictionary<string, DynamoDBEntry>> GetPhraseKeys(HashSet<string> allPhrases, string table, string id, IEnumerable<string> names)
        {
            var keys = new List<Dictionary<string, DynamoDBEntry>>();
            
            foreach (var sentence in names)
            {
                foreach (var phrase in GetPhrases(sentence))
                {
                    var detailedPhrase = $"{phrase}${id}${sentence}";
                    if (allPhrases.Contains(detailedPhrase)) continue;
                    allPhrases.Add(detailedPhrase);
                    for (var i = 0; i < 10; i++)
                        keys.Add(new Dictionary<string, DynamoDBEntry> { ["sourceTable"] = table + i, ["phrase"] = detailedPhrase });
                }
            }
            return keys;
        }

        private string[] GetPhrases(string sentence)
        {
            var matches = _phrasesRegex.Matches(sentence.ToLower());
            var phrases = new string[matches.Count];
            for (var i = 0; i < phrases.Length; i++)
                phrases[i] = matches[i].Value;
            for (var j = phrases.Length - 2; j >= 0; j--)
                phrases[j] = phrases[j] + " " + phrases[j + 1];
            return phrases;
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
