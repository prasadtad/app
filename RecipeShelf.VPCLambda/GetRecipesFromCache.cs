using RecipeShelf.Cache.Models;
using RecipeShelf.Cache;

namespace RecipeShelf.VPCLambda
{
    public sealed class GetRecipesFromCache
    {
        private readonly RecipeCache _recipeCache;

        public GetRecipesFromCache(RecipeCache recipeCache)
        {
            _recipeCache = recipeCache;
        }

        public string[] Execute(RecipeFilter input)
        {
            var ids = _recipeCache.ByFilter(input);
            var idStrings = new string[ids.Length];
            for (var i = 0; i < ids.Length; i++)
                idStrings[i] = ids[i].Value;
            return idStrings;
        }
    }
}
