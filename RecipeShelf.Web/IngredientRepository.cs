using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecipeShelf.Common;
using RecipeShelf.Common.Models;
using RecipeShelf.Common.Proxies;
using RecipeShelf.Data.Proxies;
using RecipeShelf.Data.Server.Proxies;
using RecipeShelf.Data.VPC;
using System;
using System.Threading.Tasks;

namespace RecipeShelf.Web
{
    public interface IIngredientRepository : IRepository
    {
        Task<RepositoryResponse<Ingredient>> GetAsync(string id);

        Task<RepositoryResponse<string>> CreateAsync(Ingredient ingredient);

        Task<RepositoryResponse<bool>> UpdateAsync(string id, Ingredient ingredient);
    }

    public class IngredientRepository : Repository, IIngredientRepository
    {
        private IngredientCache IngredientCache { get { return (IngredientCache)Cache; } }

        public IngredientRepository(ILogger logger, IFileProxy fileProxy, INoSqlDbProxy noSqlDbProxy, IMarkdownProxy markdownProxy, IngredientCache ingredientCache) :
            base(logger, fileProxy, noSqlDbProxy, markdownProxy, ingredientCache)
        {
        }

        public Task<RepositoryResponse<Ingredient>> GetAsync(string id)
        {
            return ExecuteAsync(() => NoSqlDbProxy.GetIngredientAsync(id), "Cannot get Ingredient " + id, Sources.NoSql);
        }

        public Task<RepositoryResponse<string>> CreateAsync(Ingredient ingredient)
        {
            return ExecuteAsync(async () =>
            {
                ingredient.Id = Helper.GenerateNewId();
                await PutAsync(ingredient);
                return ingredient.Id;
            }, "Cannot create new Ingredient", Sources.All);
        }

        public Task<RepositoryResponse<bool>> UpdateAsync(string id, Ingredient ingredient)
        {
            if (id != ingredient.Id)
                return Task.FromResult(new RepositoryResponse<bool>(error: "Id " + id + " mismatch"));
            return ExecuteAsync(async () =>
            {                
                await PutAsync(ingredient);
                return true;
            }, "Cannot update Ingredient " + id, Sources.All);
        }

        private async Task PutAsync(Ingredient ingredient)
        {
            var oldJson = await NoSqlDbProxy.GetIngredientAsync(ingredient.Id);

            var json = JsonConvert.SerializeObject(ingredient, Formatting.Indented);
            await FileProxy.PutTextAsync("ingredients/" + ingredient.Id, json);
            await NoSqlDbProxy.PutIngredientAsync(ingredient);
            IngredientCache.Store(ingredient);

        }

        protected override Task<bool> TryDeleteAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}
