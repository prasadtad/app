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

namespace RecipeShelf.Utilities
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
            var markdownProxy = _serviceProvider.GetService<IMarkdownProxy>();
            var fileProxy = _serviceProvider.GetService<IFileProxy>();
            foreach (var key in await fileProxy.ListKeysAsync("recipes"))
                await markdownProxy.PutRecipeAsync(JsonConvert.DeserializeObject<Recipe>(await fileProxy.GetTextAsync(key)));
        }
    }
}
