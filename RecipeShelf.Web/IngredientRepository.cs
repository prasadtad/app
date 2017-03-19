﻿using Microsoft.Extensions.Logging;
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
            return ExecuteAsync(async () =>
            {
                if (!Cache.Exists(id)) return new RepositoryResponse<Ingredient>(error: "Ingredient " + id + " does not exist");
                return new RepositoryResponse<Ingredient>(await NoSqlDbProxy.GetIngredientAsync(id));
            }, "Cannot get Ingredient " + id, Sources.Cache | Sources.NoSql);
        }

        public Task<RepositoryResponse<string>> CreateAsync(Ingredient ingredient)
        {
            return ExecuteAsync(async () =>
            {
                ingredient.Id = Helper.GenerateNewId();
                // To avoid the horrors of duplicate ids at all costs
                while (Cache.Exists(ingredient.Id)) ingredient.Id = Helper.GenerateNewId();
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
                if (!Cache.Exists(id)) return new RepositoryResponse<bool>(error: "Ingredient " + id + " does not exist");
                await PutAsync(ingredient, await NoSqlDbProxy.GetIngredientAsync(id));
                return new RepositoryResponse<bool>(response: true);
            }, "Cannot update Ingredient " + id, Sources.All);
        }
        
        protected async override Task<RepositoryResponse<bool>> TryDeleteAsync(string id)
        {
            if (!Cache.Exists(id)) return new RepositoryResponse<bool>(error: "Ingredient " + id + " does not exist");
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
            return new RepositoryResponse<bool>(response: true);
        }

        private async Task PutAsync(Ingredient ingredient, Ingredient oldIngredient = null)
        {
            var key = "ingredients/" + ingredient.Id;
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

        private async Task RollbackFileProxy(string key, Ingredient oldIngredient)
        {
            if (oldIngredient == null)
                await FileProxy.DeleteAsync(key);
            else
                await FileProxy.PutTextAsync(key, JsonConvert.SerializeObject(oldIngredient, Formatting.Indented));
        }

        private async Task RollbackNoSqlDbProxy(string id, Ingredient oldIngredient)
        {
            if (oldIngredient == null)
                await NoSqlDbProxy.DeleteIngredientAsync(id);
            else
                await NoSqlDbProxy.PutIngredientAsync(oldIngredient);
        }
    }
}
