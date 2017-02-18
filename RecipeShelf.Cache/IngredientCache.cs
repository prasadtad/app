using RecipeShelf.Cache.Models;
using RecipeShelf.Cache.Proxies;
using RecipeShelf.Common;
using RecipeShelf.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RecipeShelf.Cache
{
    public sealed class IngredientCache : Cache
    {
        protected override string SearchWordsKey => KeyRegistry.Ingredients.SearchWords;

        public IngredientCache(ICacheProxy cacheProxy) : base(cacheProxy, new Logger<IngredientCache>())
        {
        }

        public string[] GetCategories() => CacheProxy.Members(KeyRegistry.Ingredients.Category);

        public Id[] ByCategory(string category) => CacheProxy.Ids(KeyRegistry.Ingredients.Category.Append(category));

        public bool IsVegan(Id id) => CacheProxy.GetFlag(KeyRegistry.Ingredients.Vegan, id.Value);

        public void Store(Ingredient ingredient)
        {
            var sw = Stopwatch.StartNew();

            var oldNames = CacheProxy.Get(KeyRegistry.Ingredients.Names, ingredient.Id.Value);

            var batch = new List<IEntry>();

            batch.Add(new HashEntry(KeyRegistry.Ingredients.Names, ingredient.Id.Value, string.Join(Environment.NewLine, ingredient.Names)));

            // For each recipe which uses this ingredient
            foreach (var recipeId in CacheProxy.Ids(KeyRegistry.Recipes.IngredientId.Append(ingredient.Id.Value)))
            {
                // Get the ingredients used by the recipe
                var recipeIngredients = CacheProxy.Get(KeyRegistry.Ingredients.RecipeId, recipeId.Value);
                var vegan = true;
                foreach (var recipeIngredientId in recipeIngredients.Split(','))
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

            batch.Add(new HashEntry(KeyRegistry.Ingredients.Vegan, ingredient.Id.Value, ingredient.Vegan));

            batch.AddRange(CreateSearchWordEntries(ingredient.Id, oldNames, ingredient.Names));

            CacheProxy.Store(batch);

            Logger.Duration("Store", $"Saving {ingredient.Id.Value}", sw);
        }

    }
}
