using Newtonsoft.Json;
using RecipeShelf.Common.Models;
using RecipeShelf.Common.Proxies;
using RecipeShelf.NoSql;
using System.Threading.Tasks;

namespace RecipeShelf.Lambda
{
    public sealed class UpdateRecipeRecords
    {
        private readonly INoSqlDbProxy _noSqlDbProxy;
        private readonly IFileProxy _fileProxy;

        public UpdateRecipeRecords(INoSqlDbProxy noSqlDbProxy, IFileProxy fileProxy)
        {
            _noSqlDbProxy = noSqlDbProxy;
            _fileProxy = fileProxy;
        }

        public async Task ExecuteAsync(string key)
        {
            var text = await _fileProxy.GetTextAsync(key);
            var recipe = JsonConvert.DeserializeObject<Recipe>(text);
            await _noSqlDbProxy.PutRecipeAsync(recipe);
        }
    }
}
