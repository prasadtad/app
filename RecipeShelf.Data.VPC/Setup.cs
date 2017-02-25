using Microsoft.Extensions.DependencyInjection;
using RecipeShelf.Data.VPC.Proxies;

namespace RecipeShelf.Data.VPC
{
    public static class Setup
    {
        public static IServiceCollection AddVPCData(this IServiceCollection services)
        {
            var cacheProxy = new RedisProxy();
            var ingredientCache = new IngredientCache(cacheProxy);
            return services.AddSingleton(ingredientCache)
                           .AddSingleton(new RecipeCache(cacheProxy, ingredientCache));
        }
    }
}
