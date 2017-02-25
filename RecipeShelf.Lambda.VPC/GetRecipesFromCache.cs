using RecipeShelf.Data.VPC.Models;
using RecipeShelf.Data.VPC;
using RecipeShelf.Common.Models;
using System.Collections.Generic;

namespace RecipeShelf.Lambda.VPC
{
    public sealed class GetRecipesFromCache
    {
        private readonly RecipeCache _recipeCache;

        public GetRecipesFromCache(RecipeCache recipeCache)
        {
            _recipeCache = recipeCache;
        }

        public IEnumerable<RecipeId> Execute(RecipeFilter input)
        {
            return _recipeCache.ByFilter(input);            
        }
    }
}
