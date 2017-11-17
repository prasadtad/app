using Microsoft.Extensions.Logging;
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
        private readonly ILogger<IngredientsCache> _logger;
        private readonly IFileProxy _fileProxy;
        private readonly CommonSettings _settings;

        private IDictionary<string, Ingredient> _ingredients;
        private DateTime? _lastModified;

        private DateTime _lastAccessed = DateTime.MinValue;

        public IngredientsCache(IFileProxy fileProxy, ILogger<IngredientsCache> logger, IOptions<CommonSettings> optionsAccessor)
        {
            _fileProxy = fileProxy;
            _logger = logger;
            _settings = optionsAccessor.Value;
        }

        public async Task<Ingredient> GetAsync(string id)
        {
            return (await GetIngredientsAsync())[id];
        }

        private async Task<IDictionary<string, Ingredient>> GetIngredientsAsync()
        {
            _logger.LogDebug("Getting ingredients dictionary from cache");
            if (_ingredients == null || DateTime.Now.Subtract(_lastAccessed) > _settings.IngredientsCacheExpiration)
            {
                _ingredients = await CreateAsync();
                _lastAccessed = DateTime.Now;
            }
            return _ingredients;
        }

        private async Task<IDictionary<string, Ingredient>> CreateAsync()
        {
            _logger.LogDebug("Creating ingredients dictionary");
            var fileText = await _fileProxy.GetTextAsync("ingredients.json", _lastModified);
            _lastModified = fileText.LastModified;
            if (fileText.Text == null)
                _logger.LogDebug("Ingredients file didn't change, using already cached one");
            return fileText.Text == null ? _ingredients : Deserialize(fileText.Text);
        }

        private IDictionary<string, Ingredient> Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<IEnumerable<Ingredient>>(json).ToDictionary(i => i.Id);
        }
    }
}
