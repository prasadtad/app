using Newtonsoft.Json;
using RecipeShelf.Data.VPC;
using RecipeShelf.Common.Models;
using RecipeShelf.Common.Proxies;
using System.Threading.Tasks;

namespace RecipeShelf.Lambda.VPC
{
    public sealed class UpdateRecipeCache
    {
        private readonly IFileProxy _fileProxy;
        private readonly RecipeCache _recipeCache;

        public UpdateRecipeCache(IFileProxy fileProxy, RecipeCache recipeCache)
        {
            _fileProxy = fileProxy;
            _recipeCache = recipeCache;
        }

        public async Task ExecuteAsync(string key)
        {
            var text = await _fileProxy.GetTextAsync(key);
            var recipe = JsonConvert.DeserializeObject<Recipe>(text);
            _recipeCache.Store(recipe);            
        }
    }
}
