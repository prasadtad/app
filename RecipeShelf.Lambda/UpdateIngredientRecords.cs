using Newtonsoft.Json;
using RecipeShelf.Common.Models;
using RecipeShelf.Common.Proxies;
using RecipeShelf.Data.Proxies;
using System.Threading.Tasks;

namespace RecipeShelf.Lambda
{
    public sealed class UpdateIngredientRecords
    {
        private readonly INoSqlDbProxy _noSqlDbProxy;
        private readonly IFileProxy _fileProxy;

        public UpdateIngredientRecords(INoSqlDbProxy noSqlDbProxy, IFileProxy fileProxy)
        {
            _noSqlDbProxy = noSqlDbProxy;
            _fileProxy = fileProxy;
        }

        public async Task ExecuteAsync(string key)
        {
            var text = await _fileProxy.GetTextAsync(key);
            var ingredient = JsonConvert.DeserializeObject<Ingredient>(text);
            await _noSqlDbProxy.PutIngredientAsync(ingredient);
        }
    }
}
