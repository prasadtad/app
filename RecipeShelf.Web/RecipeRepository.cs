using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecipeShelf.Common;
using RecipeShelf.Common.Models;
using RecipeShelf.Common.Proxies;
using RecipeShelf.Data.VPC;
using RecipeShelf.Data.VPC.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipeShelf.Web
{
    public interface IRecipeRepository : IRepository
    {
        Task<RepositoryResponse<string>> CreateAsync(Recipe recipe);

        Task<RepositoryResponse<bool>> UpdateAsync(string id, Recipe recipe);

        Task<RepositoryResponse<bool>> ResetCacheAsync();

        Task<RepositoryResponse<IEnumerable<string>>> SearchNamesAsync(string sentence);

        Task<RepositoryResponse<IEnumerable<string>>> FilterAsync(RecipeFilter filter);
    }

    public class RecipeRepository : Repository, IRecipeRepository
    {
        private RecipeCache RecipeCache { get { return (RecipeCache)Cache; } }

        public RecipeRepository(ILogger<RecipeRepository> logger, IFileProxy fileProxy, RecipeCache recipeCache) :
            base(logger, fileProxy, recipeCache)
        {
        }

        public Task<RepositoryResponse<string>> CreateAsync(Recipe recipe)
        {
            return ExecuteAsync(async () =>
            {
                var newId = Helper.GenerateNewId();
                // To avoid duplicate ids at all costs
                while (await Cache.ExistsAsync(newId)) newId = Helper.GenerateNewId();
                await PutAsync(newId, recipe, null);
                return newId;
            }, "Cannot create new Recipe", Sources.All);
        }

        public Task<RepositoryResponse<bool>> UpdateAsync(string id, Recipe recipe)
        {
            if (id != recipe.Id)
                return Task.FromResult(new RepositoryResponse<bool>(error: "Id " + id + " mismatch"));
            return ExecuteAsync(async () =>
            {
                if (!await Cache.ExistsAsync(id)) return new RepositoryResponse<bool>(error: "Recipe " + id + " does not exist");
                if (!await Cache.TryLockAsync(recipe.Id)) return new RepositoryResponse<bool>(error: "Recipe " + recipe.Id + " is already being changed");
                try
                {
                    await PutAsync(id, recipe, (await FileProxy.GetTextAsync("recipes/" + id)).Text);
                }
                finally
                {
                    await Cache.UnLockAsync(recipe.Id);
                }
                return new RepositoryResponse<bool>(response: true);
            }, "Cannot update Recipe " + id, Sources.All);
        }

        public Task<RepositoryResponse<bool>> ResetCacheAsync()
        {
            return ExecuteAsync(async () =>
            {
                await RecipeCache.FlushAsync();
                foreach (var key in await FileProxy.ListKeysAsync("recipes"))
                {
                    var recipe = JsonConvert.DeserializeObject<Recipe>((await FileProxy.GetTextAsync(key)).Text);
                    await RecipeCache.StoreAsync(recipe);
                }
                return new RepositoryResponse<bool>(response: true);
            }, "Cannot reset Recipe Cache", Sources.All);
        }

        public Task<RepositoryResponse<IEnumerable<string>>> SearchNamesAsync(string sentence)
        {
            return ExecuteAsync(() => RecipeCache.SearchNames(sentence), "Cannot search Recipe Names for " + sentence, Sources.Cache);
        }

        public Task<RepositoryResponse<IEnumerable<string>>> FilterAsync(RecipeFilter filter)
        {
            return ExecuteAsync(() => RecipeCache.FilterAsync(filter), "Cannot filter Recipes with " + JsonConvert.SerializeObject(filter, Formatting.Indented), Sources.Cache);
        }

        protected async override Task<RepositoryResponse<bool>> TryDeleteAsync(string id)
        {
            if (!await Cache.ExistsAsync(id)) return new RepositoryResponse<bool>(error: "Recipe " + id + " does not exist");
            if (!await Cache.TryLockAsync(id)) return new RepositoryResponse<bool>(error: "Recipe " + id + " is already being changed");
            try
            {
                var oldRecipe = (await FileProxy.GetTextAsync("recipes/" + id)).Text;
                var key = "recipes/" + id;
                await FileProxy.DeleteAsync(key);
                try
                {
                    await RecipeCache.RemoveAsync(id);
                }
                catch (Exception)   // Rollback file
                {
                    await RollbackFileProxyAsync(key, oldRecipe);
                    throw;
                }
            }
            finally
            {
                await Cache.UnLockAsync(id);
            }
            return new RepositoryResponse<bool>(response: true);
        }

        private async Task PutAsync(string id, Recipe recipe, string oldRecipeJson)
        {
            var key = "recipes/" + id;
            recipe = recipe.With(lastModified: DateTime.UtcNow);
            await FileProxy.PutTextAsync(key, JsonConvert.SerializeObject(recipe, Formatting.Indented));
            try
            {
                await RecipeCache.StoreAsync(recipe);
            }
            catch (Exception)   // Rollback file
            {
                await RollbackFileProxyAsync(key, oldRecipeJson);
                throw;
            }
        }

        private async Task RollbackFileProxyAsync(string key, string oldRecipeJson)
        {
            if (string.IsNullOrEmpty(oldRecipeJson))
                await FileProxy.DeleteAsync(key);
            else
                await FileProxy.PutTextAsync(key, oldRecipeJson);
        }
    }
}
