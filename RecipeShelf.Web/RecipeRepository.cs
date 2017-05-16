using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecipeShelf.Common;
using RecipeShelf.Common.Models;
using RecipeShelf.Common.Proxies;
using RecipeShelf.Data.Proxies;
using RecipeShelf.Data.Server.Proxies;
using RecipeShelf.Data.VPC;
using RecipeShelf.Data.VPC.Models;
using System;
using System.Threading.Tasks;

namespace RecipeShelf.Web
{
    public interface IRecipeRepository : IRepository
    {
        Task<RepositoryResponse<Recipe>> GetAsync(string id);

        Task<RepositoryResponse<string>> CreateAsync(Recipe recipe);

        Task<RepositoryResponse<bool>> UpdateAsync(string id, Recipe recipe);
    }

    public class RecipeRepository : Repository, IRecipeRepository
    {
        private RecipeCache RecipeCache { get { return (RecipeCache)Cache; } }

        private readonly IngredientCache IngredientCache;

        public RecipeRepository(ILogger<RecipeRepository> logger, IFileProxy fileProxy, INoSqlDbProxy noSqlDbProxy, IMarkdownProxy markdownProxy, IngredientCache ingredientCache, RecipeCache recipeCache) :
            base(logger, fileProxy, noSqlDbProxy, markdownProxy, recipeCache)
        {
            IngredientCache = ingredientCache;
        }

        public Task<RepositoryResponse<Recipe>> GetAsync(string id)
        {
            return ExecuteAsync(async () =>
            {
                if (!Cache.Exists(id)) return new RepositoryResponse<Recipe>(error: "Recipe " + id + " does not exist");
                return new RepositoryResponse<Recipe>(await NoSqlDbProxy.GetRecipeAsync(id));
            }, "Cannot get Recipe " + id, Sources.Cache | Sources.NoSql);
        }

        public Task<RepositoryResponse<string>> CreateAsync(Recipe recipe)
        {
            return ExecuteAsync(async () =>
            {
                recipe.Id = Helper.GenerateNewId();
                // To avoid duplicate ids at all costs
                while (Cache.Exists(recipe.Id)) recipe.Id = Helper.GenerateNewId();
                await PutAsync(recipe);
                return recipe.Id;
            }, "Cannot create new Recipe", Sources.All);
        }

        public Task<RepositoryResponse<bool>> UpdateAsync(string id, Recipe recipe)
        {
            if (id != recipe.Id)
                return Task.FromResult(new RepositoryResponse<bool>(error: "Id " + id + " mismatch"));
            return ExecuteAsync(async () =>
            {
                if (!Cache.Exists(id)) return new RepositoryResponse<bool>(error: "Recipe " + id + " does not exist");
                if (!Cache.TryLock(recipe.Id)) return new RepositoryResponse<bool>(error: "Recipe " + recipe.Id + " is already being changed");
                try
                {
                    await PutAsync(recipe, await NoSqlDbProxy.GetRecipeAsync(id));
                }
                finally
                {
                    Cache.UnLock(recipe.Id);
                }
                return new RepositoryResponse<bool>(response: true);
            }, "Cannot update Recipe " + id, Sources.All);
        }

        protected async override Task<RepositoryResponse<bool>> TryDeleteAsync(string id)
        {
            if (!Cache.Exists(id)) return new RepositoryResponse<bool>(error: "Ingredient " + id + " does not exist");
            var recipesUsingIngredient = RecipeCache.ByFilter(new RecipeFilter { IngredientIds = new[] { id } });
            if (recipesUsingIngredient.Length > 0)
                return new RepositoryResponse<bool>(error: "Ingredient " + id + " is used by Recipes [" + string.Join(", ", recipesUsingIngredient) + "]");
            if (!Cache.TryLock(id)) return new RepositoryResponse<bool>(error: "Ingredient " + id + " is already being changed");
            try
            {                
                Ingredient oldIngredient = await NoSqlDbProxy.GetIngredientAsync(id);
                var key = "ingredients/" + id;
                await FileProxy.DeleteAsync(key);
                try
                {
                    await NoSqlDbProxy.DeleteIngredientAsync(id);
                }
                catch (Exception)   // Rollback file
                {
                    await RollbackFileProxy(key, oldIngredient);
                    throw;
                }
                try
                {
                    IngredientCache.Remove(id);
                }
                catch (Exception)   // Rollback nosql
                {
                    await RollbackFileProxy(key, oldIngredient);
                    await RollbackNoSqlDbProxy(id, oldIngredient);
                    throw;
                }
            }
            finally
            {
                Cache.UnLock(id);
            }
            return new RepositoryResponse<bool>(response: true);
        }

        private async Task PutAsync(Recipe recipe, Recipe oldRecipe = null)
        {
            var key = "ingredients/" + ingredient.Id;
            ingredient.LastModified = DateTime.UtcNow;
            await FileProxy.PutTextAsync(key, JsonConvert.SerializeObject(ingredient, Formatting.Indented));
            try
            {
                await NoSqlDbProxy.PutIngredientAsync(ingredient);
            }
            catch (Exception)   // Rollback file
            {
                await RollbackFileProxy(key, oldIngredient);
                throw;
            }
            try
            {
                IngredientCache.Store(ingredient);
            }
            catch (Exception)   // Rollback nosql
            {
                await RollbackFileProxy(key, oldIngredient);
                await RollbackNoSqlDbProxy(ingredient.Id, oldIngredient);
                throw;
            }
        }

        private async Task RollbackFileProxy(string key, Recipe oldRecipe)
        {
            if (oldIngredient == null)
                await FileProxy.DeleteAsync(key);
            else
                await FileProxy.PutTextAsync(key, JsonConvert.SerializeObject(oldIngredient, Formatting.Indented));
        }

        private async Task RollbackNoSqlDbProxy(string id, Recipe oldRecipe)
        {
            if (oldIngredient == null)
                await NoSqlDbProxy.DeleteIngredientAsync(id);
            else
                await NoSqlDbProxy.PutIngredientAsync(oldIngredient);
        }
    }
}
