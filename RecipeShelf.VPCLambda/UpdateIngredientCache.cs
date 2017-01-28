using Newtonsoft.Json;
using RecipeShelf.Cache;
using RecipeShelf.Common.Models;
using RecipeShelf.Common.Proxies;
using System.Threading.Tasks;

namespace RecipeShelf.VPCLambda
{
    public sealed class UpdateIngredientCache
    {
        private readonly IFileProxy _fileProxy;
        private readonly IngredientCache _ingredientCache;

        public UpdateIngredientCache(IFileProxy fileProxy, IngredientCache ingredientCache)
        {
            _fileProxy = fileProxy;
            _ingredientCache = ingredientCache;
        }

        public async Task ExecuteAsync(string key)
        {
            var text = await _fileProxy.GetTextAsync(key);
            var ingredient = JsonConvert.DeserializeObject<Ingredient>(text);          
            _ingredientCache.Store(ingredient);
        }
    }
}
