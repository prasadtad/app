using RecipeShelf.Cache.Models;
using RecipeShelf.Cache.Proxies;
using RecipeShelf.Common.Models;
using System;
using System.Collections.Generic;

namespace RecipeShelf.Cache
{
    public class IngredientCache
    {
        private ICacheProxy _cacheProxy;

        public IngredientCache(ICacheProxy cacheProxy)
        {
            _cacheProxy = cacheProxy;
        }

        public string[] GetCategories() => _cacheProxy.Members(KeyRegistry.Ingredients.Category);

        public Id[] ByCategory(string category) => _cacheProxy.Ids(KeyRegistry.Ingredients.Category.Append(category));

        public bool IsVegan(Id id) => _cacheProxy.GetFlag(KeyRegistry.Ingredients.Vegan, id);

        public void Store(Ingredient ingredient)
        {
            var batch = new List<IEntry>();

            // For each recipe which uses this ingredient
            foreach (var recipeId in _cacheProxy.Ids(KeyRegistry.Recipes.IngredientId.Append(ingredient.Id.Value)))
            {
                // Get the ingredients used by the recipe
                var recipeIngredients = _cacheProxy.Get(KeyRegistry.Ingredients.RecipeId, recipeId);
                var vegan = true;
                foreach (var recipeIngredientId in recipeIngredients.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (IsVegan(new Id(recipeIngredientId))) continue;                    
                    vegan = false;
                    break;
                }
                // Update recipe vegan status
                batch.Add(new SetEntry(KeyRegistry.Recipes.Vegan, vegan, recipeId));
            }

            var category = !string.IsNullOrEmpty(ingredient.Category) ? ingredient.Category : IngredientKeys.DEFAULT_CATEGORY;
            batch.Add(new SetEntry(KeyRegistry.Ingredients.Category, category, ingredient.Id));

            batch.Add(new HashEntry(KeyRegistry.Ingredients.Vegan, ingredient.Id, ingredient.Vegan));

            _cacheProxy.Store(batch);
        }
    }
}
