using RecipeShelf.Common.Proxies;
using System.Threading.Tasks;

namespace RecipeShelf.Lambda.VPC
{
    public sealed class UpdateRecipeCacheFull
    {
        private readonly IFileProxy _fileProxy;

        private readonly UpdateRecipeCache _updateRecipeCache;

        public UpdateRecipeCacheFull(IFileProxy fileProxy, UpdateRecipeCache updateRecipeCache)
        {
            _fileProxy = fileProxy;
            _updateRecipeCache = updateRecipeCache;
        }

        public async Task ExecuteAsync()
        {
            foreach (var key in (await _fileProxy.ListKeysAsync("recipes")))
                await _updateRecipeCache.ExecuteAsync(key);
        }
    }
}
