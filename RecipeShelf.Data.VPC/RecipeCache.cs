using Microsoft.Extensions.Logging;
using RecipeShelf.Common;
using RecipeShelf.Common.Models;
using RecipeShelf.Data.VPC.Models;
using RecipeShelf.Data.VPC.Proxies;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipeShelf.Data.VPC
{
    public sealed class RecipeCache : Cache
    {
        public override string Table => "Recipes";

        private IngredientsCache _ingredientsCache;

        protected override string NamesKey => KeyRegistry.Recipes.Names;

        protected override string SearchWordsKey => KeyRegistry.Recipes.SearchWords;

        protected override string LocksKey => KeyRegistry.Recipes.Locks;

        public RecipeCache(ICacheProxy cacheProxy, IngredientsCache ingredientsCache, ILogger<RecipeCache> logger) : base(cacheProxy, logger)
        {
            _ingredientsCache = ingredientsCache;
        }

        public async Task<IEnumerable<string>> ByChefAsync(string chefId) => await CacheProxy.MembersAsync(KeyRegistry.Recipes.ChefId.Append(chefId));

        public async Task<IEnumerable<string>> FilterAsync(RecipeFilter filter)
        {
            var keys = new List<string>();
            if (filter.Vegan != null) keys.Add(KeyRegistry.Recipes.Vegan.Append(filter.Vegan.Value));
            if (filter.OvernightPreparation != null) keys.Add(KeyRegistry.Recipes.OvernightPreparation.Append(filter.OvernightPreparation.Value));
            if (filter.IngredientIds != null && filter.IngredientIds.Length > 0) keys.Add(await CacheProxy.CombineAsync(new CombineOptions(LogicalOperator.Or, KeyRegistry.Recipes.IngredientId, filter.IngredientIds)));
            if (filter.Regions != null && filter.Regions.Length > 0) keys.Add(await CacheProxy.CombineAsync(new CombineOptions(LogicalOperator.Or, KeyRegistry.Recipes.Region, filter.Regions)));
            if (filter.Cuisines != null && filter.Cuisines.Length > 0) keys.Add(await CacheProxy.CombineAsync(new CombineOptions(LogicalOperator.Or, KeyRegistry.Recipes.Cuisine, filter.Cuisines)));
            if (filter.SpiceLevels != null && filter.SpiceLevels.Length > 0) keys.Add(await CacheProxy.CombineAsync(new CombineOptions(LogicalOperator.Or, KeyRegistry.Recipes.SpiceLevel, filter.SpiceLevels.ToStrings())));
            if (filter.TotalTimes != null && filter.TotalTimes.Length > 0) keys.Add(await CacheProxy.CombineAsync(new CombineOptions(LogicalOperator.Or, KeyRegistry.Recipes.TotalTime, filter.TotalTimes.ToStrings())));
            if (filter.Collections != null && filter.Collections.Length > 0) keys.Add(await CacheProxy.CombineAsync(new CombineOptions(LogicalOperator.Or, KeyRegistry.Recipes.Collection, filter.Collections)));
            if (keys.Count == 0) return new string[0];
            return await CacheProxy.MembersAsync(await CacheProxy.CombineAsync(new CombineOptions(LogicalOperator.And, keys.ToArray())));
        }

        public Task<bool> IsVeganAsync(string id) => CacheProxy.IsMemberAsync(KeyRegistry.Recipes.Vegan.Append(true), id);

        public async Task<IEnumerable<string>> GetChefsAsync() => await CacheProxy.MembersAsync(KeyRegistry.Recipes.ChefId);

        public async Task<IEnumerable<string>> GetCollectionsAsync() => await CacheProxy.MembersAsync(KeyRegistry.Recipes.Collection);

        public async Task<IEnumerable<string>> GetCuisinesAsync() => await CacheProxy.MembersAsync(KeyRegistry.Recipes.Cuisine);

        public async Task<IEnumerable<string>> GetRegionsAsync() => await CacheProxy.MembersAsync(KeyRegistry.Recipes.Region);

        public Task<long> GetCountForCollectionAsync(string collection)
        {
            return CacheProxy.CountAsync(KeyRegistry.Recipes.Collection.Append(collection));
        }

        public async Task StoreAsync(Recipe recipe)
        {
            Logger.LogDebug("Saving Recipe {Id} in cache", recipe.Id);

            var oldNames = await CacheProxy.GetAsync(KeyRegistry.Recipes.Names, recipe.Id);

            var vegan = true;   // Store if recipe is vegan
            foreach (var ingredientId in recipe.IngredientIds)
            {
                var ingredient = await _ingredientsCache.GetAsync(ingredientId);
                if (!ingredient.Vegan)
                {
                    vegan = false;
                    break;
                }
            }

            var batch = new List<IEntry>
            {
                new HashEntry(KeyRegistry.Recipes.Names, recipe.Id, string.Join(Environment.NewLine, recipe.Names)),
                new SetEntry(KeyRegistry.Recipes.Vegan, vegan, recipe.Id),
                new SetEntry(KeyRegistry.Recipes.IngredientId, recipe.IngredientIds, recipe.Id),
                new SetEntry(KeyRegistry.Recipes.OvernightPreparation, recipe.OvernightPreparation, recipe.Id),
                new SetEntry(KeyRegistry.Recipes.Region, recipe.Region, recipe.Id),
                new SetEntry(KeyRegistry.Recipes.Cuisine, recipe.Cuisine, recipe.Id),
                new SetEntry(KeyRegistry.Recipes.SpiceLevel, recipe.SpiceLevel.ToString(), recipe.Id),
                new SetEntry(KeyRegistry.Recipes.TotalTime, recipe.TotalTime.ToString(), recipe.Id),
                new SetEntry(KeyRegistry.Recipes.Collection, recipe.Collections, recipe.Id)
            };
            batch.AddRange(await CreateSearchPhrasesAsync(recipe.Id, oldNames?.Split(Environment.NewLine) ?? new string[0], recipe.Names));

            await CacheProxy.StoreAsync(batch);
        }

        public async Task FlushAsync()
        {
            await CacheProxy.FlushAsync();
        }

        public async Task RemoveAsync(string id)
        {
            Logger.LogDebug("Removing Recipe {Id} from cache", id);

            var oldNames = await CacheProxy.GetAsync(KeyRegistry.Recipes.Names, id);

            var batch = new List<IEntry>
            {
                new HashEntry(KeyRegistry.Recipes.Names, id),
                new SetEntry(KeyRegistry.Recipes.Vegan, id),
                new SetEntry(KeyRegistry.Recipes.IngredientId, id),
                new SetEntry(KeyRegistry.Recipes.OvernightPreparation, id),
                new SetEntry(KeyRegistry.Recipes.Region, id),
                new SetEntry(KeyRegistry.Recipes.Cuisine, id),
                new SetEntry(KeyRegistry.Recipes.SpiceLevel, id),
                new SetEntry(KeyRegistry.Recipes.TotalTime, id),
                new SetEntry(KeyRegistry.Recipes.Collection, id)
            };
            batch.AddRange(await CreateSearchPhrasesAsync(id, oldNames?.Split(Environment.NewLine) ?? new string[0], new string[0]));

            await CacheProxy.StoreAsync(batch);
        }
    }
}