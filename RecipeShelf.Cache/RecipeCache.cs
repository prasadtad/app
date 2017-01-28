using RecipeShelf.Cache.Models;
using RecipeShelf.Cache.Proxies;
using RecipeShelf.Common;
using RecipeShelf.Common.Models;
using System.Collections.Generic;

namespace RecipeShelf.Cache
{
    public class RecipeCache
    {
        private ICacheProxy _cacheProxy;
        private IngredientCache _ingredientCache;

        public RecipeCache(ICacheProxy cacheProxy, IngredientCache ingredientCache)
        {
            _cacheProxy = cacheProxy;
            _ingredientCache = ingredientCache;
        }

        public Id[] ByChef(string chefId) => _cacheProxy.Ids(KeyRegistry.Recipes.ChefId.Append(chefId));

        public Id[] ByFilter(RecipeFilter filter)
        {
            var keys = new List<string>();
            if (filter.Vegan != null) keys.Add(KeyRegistry.Recipes.Vegan.Append(filter.Vegan.Value));
            if (filter.OvernightPreparation != null) keys.Add(KeyRegistry.Recipes.OvernightPreparation.Append(filter.OvernightPreparation.Value));
            if (filter.IngredientIds != null && filter.IngredientIds.Length > 0) keys.Add(_cacheProxy.Combine(new CombineOptions(LogicalOperator.Or, KeyRegistry.Recipes.IngredientId, filter.IngredientIds)));
            if (filter.Regions != null && filter.Regions.Length > 0) keys.Add(_cacheProxy.Combine(new CombineOptions(LogicalOperator.Or, KeyRegistry.Recipes.Region, filter.Regions)));
            if (filter.Cuisines != null && filter.Cuisines.Length > 0) keys.Add(_cacheProxy.Combine(new CombineOptions(LogicalOperator.Or, KeyRegistry.Recipes.Cuisine, filter.Cuisines)));
            if (filter.SpiceLevels != null && filter.SpiceLevels.Length > 0) keys.Add(_cacheProxy.Combine(new CombineOptions(LogicalOperator.Or, KeyRegistry.Recipes.SpiceLevel, filter.SpiceLevels.ToStrings())));
            if (filter.TotalTimes != null && filter.TotalTimes.Length > 0) keys.Add(_cacheProxy.Combine(new CombineOptions(LogicalOperator.Or, KeyRegistry.Recipes.TotalTime, filter.TotalTimes.ToStrings())));
            if (filter.Collections != null && filter.Collections.Length > 0) keys.Add(_cacheProxy.Combine(new CombineOptions(LogicalOperator.Or, KeyRegistry.Recipes.Collection, filter.Collections)));
            if (keys.Count == 0) return new Id[0];
            return _cacheProxy.Ids(_cacheProxy.Combine(new CombineOptions(LogicalOperator.And, keys.ToArray())));
        }

        public bool IsVegan(Id id) => _cacheProxy.IsMember(KeyRegistry.Recipes.Vegan.Append(true), id);

        public string[] GetChefs() => _cacheProxy.Members(KeyRegistry.Recipes.ChefId);

        public string[] GetCollections() => _cacheProxy.Members(KeyRegistry.Recipes.Collection);

        public string GetMainName(Id accompanimentId) => _cacheProxy.Get(KeyRegistry.Recipes.MainName, accompanimentId);

        public string[] GetCuisines() => _cacheProxy.Members(KeyRegistry.Recipes.Cuisine);

        public string[] GetRegions() => _cacheProxy.Members(KeyRegistry.Recipes.Region);

        public long GetCountForCollection(string collection)
        {
            return _cacheProxy.Count(KeyRegistry.Recipes.Collection.Append(collection));
        }

        public void Store(Recipe recipe)
        {
            var batch = new List<IEntry>();

            batch.Add(new HashEntry(KeyRegistry.Recipes.MainName, recipe.Id, recipe.Names[0]));

            // Store list of ingredientIds with recipeId as key
            batch.Add(new HashEntry(KeyRegistry.Ingredients.RecipeId, recipe.Id, string.Join(",", recipe.IngredientIds.ToStrings())));

            var vegan = true;   // Store if recipe is vegan
            foreach (var ingredientId in recipe.IngredientIds)
            {
                if (_ingredientCache.IsVegan(ingredientId)) continue;
                vegan = false;
                break;
            }
            batch.Add(new SetEntry(KeyRegistry.Recipes.Vegan, vegan, recipe.Id));

            batch.Add(new SetEntry(KeyRegistry.Recipes.IngredientId, recipe.IngredientIds.ToStrings(), recipe.Id));

            batch.Add(new SetEntry(KeyRegistry.Recipes.OvernightPreparation, recipe.OvernightPreparation, recipe.Id));

            batch.Add(new SetEntry(KeyRegistry.Recipes.Region, recipe.Region, recipe.Id));

            batch.Add(new SetEntry(KeyRegistry.Recipes.Cuisine, recipe.Cuisine, recipe.Id));

            batch.Add(new SetEntry(KeyRegistry.Recipes.SpiceLevel, recipe.SpiceLevel.ToString(), recipe.Id));

            batch.Add(new SetEntry(KeyRegistry.Recipes.TotalTime, recipe.TotalTime.ToString(), recipe.Id));

            batch.Add(new SetEntry(KeyRegistry.Recipes.Collection, recipe.Collections, recipe.Id));

            _cacheProxy.Store(batch);
        }
    }
}