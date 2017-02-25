using RecipeShelf.Common.Proxies;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeShelf.Lambda.VPC
{
    public sealed class UpdateIngredientCacheFull
    {
        private readonly IFileProxy _fileProxy;

        private readonly UpdateIngredientCache _updateIngredientCache;

        public UpdateIngredientCacheFull(IFileProxy fileProxy, UpdateIngredientCache updateIngredientCache)
        {
            _fileProxy = fileProxy;
            _updateIngredientCache = updateIngredientCache;
        }

        public async Task ExecuteAsync()
        {
            foreach (var key in (await _fileProxy.ListKeysAsync("ingredients")))
                await _updateIngredientCache.ExecuteAsync(key);
        }
    }
}
