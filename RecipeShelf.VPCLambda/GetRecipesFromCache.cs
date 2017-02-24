using RecipeShelf.Cache.Models;
using RecipeShelf.Cache;
using RecipeShelf.Common.Models;
using System.Collections.Generic;

namespace RecipeShelf.VPCLambda
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
