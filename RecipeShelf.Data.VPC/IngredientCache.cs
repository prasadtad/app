using RecipeShelf.Data.VPC.Models;
using RecipeShelf.Data.VPC.Proxies;
using RecipeShelf.Common.Models;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace RecipeShelf.Data.VPC
{
    public sealed class IngredientCache : Cache
    {
        public override string Table => "Ingredients";

        protected override string NamesKey => KeyRegistry.Ingredients.Names;

        protected override string SearchWordsKey => KeyRegistry.Ingredients.SearchWords;

        protected override string LocksKey => KeyRegistry.Ingredients.Locks;

        public IngredientCache(ICacheProxy cacheProxy, ILogger<IngredientCache> logger) : base(cacheProxy, logger)
        {
        }

        public string[] GetCategories() => CacheProxy.Members(KeyRegistry.Ingredients.Category);

        public string[] ByCategory(string category) => CacheProxy.Members(KeyRegistry.Ingredients.Category.Append(category));

        public override bool IsVegan(string id) => CacheProxy.GetFlag(KeyRegistry.Ingredients.Vegan, id);

        public void Store(Ingredient ingredient)
        {
            Logger.LogDebug("Saving Ingredient {Id} in cache", ingredient.Id);

            var oldNames = CacheProxy.Get(KeyRegistry.Ingredients.Names, ingredient.Id);

            // For each recipe which uses this ingredient
            var recipeVeganEntries = CacheProxy.Members(KeyRegistry.Recipes.IngredientId.Append(ingredient.Id))
                                               .Select(recipeId =>
                                               {
                                                   // Get the ingredients used by the recipe
                                                   var ingredientIds = CacheProxy.Get(KeyRegistry.Ingredients.RecipeId, recipeId);
                                                   var vegan = true;
                                                   foreach (var ingredientId in ingredientIds.Split(','))
                                                   {
                                                       if (IsVegan(ingredientId)) continue;
                                                       vegan = false;
                                                       break;
                                                   }
                                                   // Update recipe vegan status
                                                   return (IEntry)new SetEntry(KeyRegistry.Recipes.Vegan, vegan, recipeId);
                                               });

            var batch = new List<IEntry>
            {
                new HashEntry(KeyRegistry.Ingredients.Names, ingredient.Id, string.Join(Environment.NewLine, ingredient.Names)),
                new SetEntry(KeyRegistry.Ingredients.Category, !string.IsNullOrEmpty(ingredient.Category) ? ingredient.Category : IngredientKeys.DEFAULT_CATEGORY, ingredient.Id),
                new HashEntry(KeyRegistry.Ingredients.Vegan, ingredient.Id, ingredient.Vegan)
            };
            batch.AddRange(recipeVeganEntries);
            batch.AddRange(CreateSearchWordEntries(ingredient.Id, oldNames, ingredient.Names));

            CacheProxy.Store(batch);
        }

        public void Remove(string id)
        {
            Logger.LogDebug("Removing Ingredient {Id} from cache", id);

            var oldNames = CacheProxy.Get(KeyRegistry.Ingredients.Names, id);

            var batch = new List<IEntry>
            {
                new HashEntry(KeyRegistry.Ingredients.Names, id),
                new SetEntry(KeyRegistry.Recipes.Vegan, id),
                new SetEntry(KeyRegistry.Ingredients.Category, id),
                new HashEntry(KeyRegistry.Ingredients.Vegan, id)
            };
            batch.AddRange(CreateSearchWordEntries(id, oldNames, new string[0]));

            CacheProxy.Store(batch);
        }
    }
}
