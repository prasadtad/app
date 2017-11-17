using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RecipeShelf.Common;
using RecipeShelf.Common.Models;
using RecipeShelf.Common.Proxies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace RecipeShelf.Tests.Common
{
    public class IngredientsCacheTests : IDisposable
    {
        private readonly string _localFileProxyFolder = Path.Combine(Directory.GetCurrentDirectory(), "IngredientsCacheTestData");

        public IngredientsCacheTests()
        {
            if (!Directory.Exists(_localFileProxyFolder)) Directory.CreateDirectory(_localFileProxyFolder);
        }

        [Fact]
        public async Task GetTestAsync()
        {
            var ingredients = new List<Ingredient>
            {
                new Ingredient("a", DateTime.Now.AddDays(-2), new[] { "A" }, "A", "X", false),
                new Ingredient("b", DateTime.Now.AddMinutes(-2), new[] { "B" }, "B1", "Y", true)
            };
            await File.WriteAllTextAsync(Path.Combine(_localFileProxyFolder, "ingredients.json"), JsonConvert.SerializeObject(ingredients));
            
            var fileProxy = new LocalFileProxy(new MockLogger<LocalFileProxy>(), new MockOptions<CommonSettings>(
                                         new CommonSettings
                                         {
                                             FileProxyType = FileProxyTypes.Local,
                                             LocalFileProxyFolder = _localFileProxyFolder
                                         }));

            var ingredientsCache = new IngredientsCache(new MemoryCache(new MemoryCacheOptions()), fileProxy, new MockOptions<CommonSettings>(new CommonSettings
            {
                IngredientsCacheExpiration = TimeSpan.FromMilliseconds(500)
            }));

            var ingredient = await ingredientsCache.GetAsync("b");
            Assert.Equal("B1",ingredient.Description);
            
            ingredients.Remove(ingredient);
            ingredients.Add(ingredient.With(description: "B2"));
            await File.WriteAllTextAsync(Path.Combine(_localFileProxyFolder, "ingredients.json"), JsonConvert.SerializeObject(ingredients));

            ingredient = await ingredientsCache.GetAsync("b");
            Assert.Equal("B1", ingredient.Description);

            await Task.Delay(500);
            ingredient = await ingredientsCache.GetAsync("b");
            Assert.Equal("B2", ingredient.Description);            
        }

        public void Dispose()
        {
            if (Directory.Exists(_localFileProxyFolder))
                Directory.Delete(_localFileProxyFolder, true);
        }
    }
}
