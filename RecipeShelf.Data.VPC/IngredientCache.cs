using RecipeShelf.Data.VPC.Models;
using RecipeShelf.Data.VPC.Proxies;
using RecipeShelf.Common;
using RecipeShelf.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RecipeShelf.Data.VPC
{
    public sealed class IngredientCache : Cache
    {
        protected override string SearchWordsKey => KeyRegistry.Ingredients.SearchWords;

        public IngredientCache(ICacheProxy cacheProxy) : base(cacheProxy, new Logger<IngredientCache>())
        {
        }

        public string[] GetCategories() => CacheProxy.Members(KeyRegistry.Ingredients.Category);

        public IEnumerable<IngredientId> ByCategory(string category) => CacheProxy.Members(KeyRegistry.Ingredients.Category.Append(category)).Cast<IngredientId>();

        public bool IsVegan(IngredientId id) => CacheProxy.GetFlag(KeyRegistry.Ingredients.Vegan, id);

        public void Store(Ingredient ingredient)
        {
            var sw = Stopwatch.StartNew();

            var oldNames = CacheProxy.Get(KeyRegistry.Ingredients.Names, ingredient.Id);

            var batch = new List<IEntry>();

            batch.Add(new HashEntry(KeyRegistry.Ingredients.Names, ingredient.Id, string.Join(Environment.NewLine, ingredient.Names)));

            // For each recipe which uses this ingredient
            foreach (var recipeId in CacheProxy.Members(KeyRegistry.Recipes.IngredientId.Append(ingredient.Id)))
            {
                // Get the ingredients used by the recipe
                var recipeIngredients = CacheProxy.Get(KeyRegistry.Ingredients.RecipeId, recipeId);
                var vegan = true;
                foreach (var recipeIngredientId in recipeIngredients.Split(','))
                {
                    if (IsVegan(recipeIngredientId)) continue;
                    vegan = false;
                    break;
                }
                // Update recipe vegan status
                batch.Add(new SetEntry(KeyRegistry.Recipes.Vegan, vegan, recipeId));
            }

            var category = !string.IsNullOrEmpty(ingredient.Category) ? ingredient.Category : IngredientKeys.DEFAULT_CATEGORY;
            batch.Add(new SetEntry(KeyRegistry.Ingredients.Category, category, ingredient.Id));

            batch.Add(new HashEntry(KeyRegistry.Ingredients.Vegan, ingredient.Id, ingredient.Vegan));

            batch.AddRange(CreateSearchWordEntries(ingredient.Id, oldNames, ingredient.Names));

            CacheProxy.Store(batch);

            Logger.Duration("Store", $"Saving {ingredient.Id}", sw);
        }

        public IEnumerable<IngredientId> Search(string sentence)
        {
            return SearchNames(sentence).Cast<IngredientId>();
        }
    }
}
