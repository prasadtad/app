using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RecipeShelf.Data.VPC;
using RecipeShelf.Common;
using RecipeShelf.Common.Models;
using RecipeShelf.Common.Proxies;
using RecipeShelf.Data;
using RecipeShelf.Data.Server;
using System;
using System.Threading.Tasks;
using RecipeShelf.Data.Proxies;
using RecipeShelf.Data.Server.Proxies;
using System.Security.Cryptography;

namespace RecipeShelf.Persistence
{
    public sealed class Program
    {
        private static IServiceProvider _serviceProvider;
        private static Logger<Program> _logger;

        static Program()
        {
            _logger = new Logger<Program>();
            _serviceProvider = new ServiceCollection()
                                    .AddCommon()
                                    .AddData()
                                    .AddVPCData()
                                    .AddServerData()
                                    .BuildServiceProvider();
        }

        public static void Main(string[] args)
        {
            try
            {
                MainAsync(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }

        private static Task MainAsync(string[] args)
        {
            var fileProxy = _serviceProvider.GetService<IFileProxy>();
            var noSqlDbProxy = _serviceProvider.GetService<INoSqlDbProxy>();
            var ingredientCache = _serviceProvider.GetService<IngredientCache>();
            var recipeCache = _serviceProvider.GetService<RecipeCache>();
            var distributedQueueProxy = _serviceProvider.GetService<IDistributedQueueProxy>();
            var markdownProxy = _serviceProvider.GetService<IMarkdownProxy>();

            return distributedQueueProxy.ProcessMessagesAsync("data-updates", async messages =>
            {
                foreach (var message in messages)
                {
                    var table = message.Attributes["Table"];
                    if (table == "Recipes")
                        message.Processed = await PersistRecipeAsync(JsonConvert.DeserializeObject<Recipe>(message.Body), fileProxy, noSqlDbProxy, recipeCache, markdownProxy);
                    else if (table == "Ingredients")
                        message.Processed = await PersistIngredientAsync(JsonConvert.DeserializeObject<Ingredient>(message.Body), fileProxy, noSqlDbProxy, ingredientCache);
                }
            });
        }

        private static async Task<bool> PersistIngredientAsync(Ingredient ingredient, IFileProxy fileProxy, INoSqlDbProxy noSqlProxy, IngredientCache ingredientCache)
        {
            if (!await fileProxy.CanConnectAsync() ||
                !await noSqlProxy.CanConnectAsync() ||
                !ingredientCache.CanConnect()) return false;
            if (ingredient.Id == null) ingredient.Id = GetNewId();
            await fileProxy.PutTextAsync($"ingredients/{ingredient.Id}.json", JsonConvert.SerializeObject(ingredient, Formatting.Indented));
            await noSqlProxy.PutIngredientAsync(ingredient);
            ingredientCache.Store(ingredient);
            return true;
        }

        private static async Task<bool> PersistRecipeAsync(Recipe recipe, IFileProxy fileProxy, INoSqlDbProxy noSqlProxy, RecipeCache recipeCache, IMarkdownProxy markdownProxy)
        {
            if (!await fileProxy.CanConnectAsync() ||
                !await noSqlProxy.CanConnectAsync() ||
                !recipeCache.CanConnect() ||
                !markdownProxy.CanConnect()) return false;
            if (recipe.Id == null) recipe.Id = GetNewId();
            await fileProxy.PutTextAsync($"recipes/{recipe.Id}.json", JsonConvert.SerializeObject(recipe, Formatting.Indented));
            await noSqlProxy.PutRecipeAsync(recipe);
            recipeCache.Store(recipe);
            await markdownProxy.PutRecipeMarkdownAsync(recipe);
            return true;
        }

        private static string GetNewId()
        {
            var length = 8;
            var numBytes = (int)(Math.Log(Math.Pow(64, length)) / Math.Log(2) / 8);
            return Convert.ToBase64String(GenerateRandomBytes(numBytes)).Replace('/', '_').Replace('+', '-').Replace("=", string.Empty).Substring(0, length);
        }

        private static byte[] GenerateRandomBytes(int length)
        {
            // Create a buffer
            byte[] randBytes;

            if (length >= 1)
                randBytes = new byte[length];
            else
                randBytes = new byte[1];

            // Create a new RNGCryptoServiceProvider.
            var rand = RandomNumberGenerator.Create();

            // Fill the buffer with random bytes.
            rand.GetBytes(randBytes);

            // return the bytes.
            return randBytes;
        }
    }
}
