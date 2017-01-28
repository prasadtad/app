using RecipeShelf.Common.Proxies;
using System.Threading.Tasks;

namespace RecipeShelf.Lambda
{
    public sealed class UpdateRecipeRecordsFull
    {
        private readonly IFileProxy _fileProxy;

        private readonly UpdateRecipeRecords _updateRecipeRecords;

        public UpdateRecipeRecordsFull(IFileProxy fileProxy, UpdateRecipeRecords updateRecipeRecords)
        {
            _fileProxy = fileProxy;
            _updateRecipeRecords = updateRecipeRecords;
        }

        public async Task ExecuteAsync()
        {
            foreach (var key in await _fileProxy.ListKeysAsync("recipes"))
                await _updateRecipeRecords.ExecuteAsync(key);
        }
    }
}
