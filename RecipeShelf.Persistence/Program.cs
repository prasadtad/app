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
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            var fileProxy = _serviceProvider.GetService<IFileProxy>();
            var noSqlDbProxy = _serviceProvider.GetService<INoSqlDbProxy>();
            var ingredientCache = _serviceProvider.GetService<IngredientCache>();
            var recipeCache = _serviceProvider.GetService<RecipeCache>();
            var distributedQueueProxy = _serviceProvider.GetService<IDistributedQueueProxy>();
            var markdownProxy = _serviceProvider.GetService<IMarkdownProxy>();

            foreach (var key in await fileProxy.ListKeysAsync("ingredients"))
            {
                var text = await fileProxy.GetTextAsync(key);
                var ingredient = JsonConvert.DeserializeObject<Ingredient>(text);
                await noSqlDbProxy.PutIngredientAsync(ingredient);
                ingredientCache.Store(ingredient);
                await Task.Delay(1000);
            }

            foreach (var key in await fileProxy.ListKeysAsync("recipes"))
            {
                var text = await fileProxy.GetTextAsync(key);
                var recipe = JsonConvert.DeserializeObject<Recipe>(text);
                await noSqlDbProxy.PutRecipeAsync(recipe);
                recipeCache.Store(recipe);
                await Task.Delay(1000);
            }
        }
    }
}
