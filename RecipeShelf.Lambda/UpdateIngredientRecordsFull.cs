using RecipeShelf.Common.Proxies;
using System.Threading.Tasks;

namespace RecipeShelf.Lambda
{
    public sealed class UpdateIngredientRecordsFull
    {
        private readonly IFileProxy _fileProxy;

        private readonly UpdateIngredientRecords _updateIngredientRecords;

        public UpdateIngredientRecordsFull(IFileProxy fileProxy, UpdateIngredientRecords updateIngredientRecords)
        {
            _fileProxy = fileProxy;
            _updateIngredientRecords = updateIngredientRecords;
        }

        public async Task ExecuteAsync()
        {
            foreach (var key in await _fileProxy.ListKeysAsync("ingredients"))
                await _updateIngredientRecords.ExecuteAsync(key);
        }
    }
}
