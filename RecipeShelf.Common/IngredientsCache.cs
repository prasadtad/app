using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RecipeShelf.Common.Models;
using RecipeShelf.Common.Proxies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeShelf.Common
{
    public class IngredientsCache
    {
        private readonly IMemoryCache _cache;
        private readonly IFileProxy _fileProxy;
        private readonly CommonSettings _settings;

        private const string INGREDIENTS_CACHE_KEY = "IngredientsCacheKey";
        private const string INGREDIENTS_CACHE_LASTMODIFIED_KEY = "IngredientsCacheLastModifiedKey";

        public IngredientsCache(IMemoryCache cache, IFileProxy fileProxy, IOptions<CommonSettings> optionsAccessor)
        {
            _cache = cache;
            _fileProxy = fileProxy;
            _settings = optionsAccessor.Value;
        }

        public async Task<Ingredient> GetAsync(string id)
        {
            return (await GetIngredientsAsync())[id];
        }

        private Task<IDictionary<string, Ingredient>> GetIngredientsAsync()
        {
            return _cache.GetOrCreateAsync(INGREDIENTS_CACHE_KEY, CreateAsync);
        }

        private async Task<IDictionary<string, Ingredient>> CreateAsync(ICacheEntry ce)
        {
            ce.SetAbsoluteExpiration(_settings.IngredientsCacheExpiration);
            var fileText = await _fileProxy.GetTextAsync("ingredients.json", _cache.Get<DateTime?>(INGREDIENTS_CACHE_LASTMODIFIED_KEY));
            _cache.Set<DateTime?>(INGREDIENTS_CACHE_LASTMODIFIED_KEY, fileText.LastModified);
            return fileText.Text == null ? (IDictionary<string, Ingredient>)ce.Value : Deserialize(fileText.Text);
        }

        private IDictionary<string, Ingredient> Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<IEnumerable<Ingredient>>(json).ToDictionary(i => i.Id);
        }
    }
}
