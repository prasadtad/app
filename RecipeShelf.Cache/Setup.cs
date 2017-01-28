using Microsoft.Extensions.DependencyInjection;
using RecipeShelf.Cache.Proxies;

namespace RecipeShelf.Cache
{
    public static class Setup
    {
        public static IServiceCollection AddCaching(this IServiceCollection services)
        {
            var cacheProxy = new RedisProxy();
            var ingredientCache = new IngredientCache(cacheProxy);
            return services.AddSingleton(ingredientCache)
                           .AddSingleton(new RecipeCache(cacheProxy, ingredientCache));
        }
    }
}
