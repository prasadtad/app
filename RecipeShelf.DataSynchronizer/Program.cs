using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RecipeShelf.Cache;
using RecipeShelf.Common;
using RecipeShelf.Common.Models;
using RecipeShelf.Common.Proxies;
using RecipeShelf.NoSql;
using RecipeShelf.Site;
using System;
using System.Threading.Tasks;

namespace RecipeShelf.DataSynchronizer
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
                                    .AddNoSql()
                                    .AddCaching()
                                    .AddSite()
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
