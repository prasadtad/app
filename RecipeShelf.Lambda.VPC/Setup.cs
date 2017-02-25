using Microsoft.Extensions.DependencyInjection;

namespace RecipeShelf.Lambda.VPC
{
    public static class Setup
    {
        public static IServiceCollection AddVPCLambda(this IServiceCollection services)
        {
            return services.AddSingleton<GetRecipesFromCache>()
                           .AddSingleton<UpdateIngredientCache>()
                           .AddSingleton<UpdateRecipeCache>()
                           .AddSingleton<UpdateIngredientCacheFull>()
                           .AddSingleton<UpdateRecipeCacheFull>();
        }
    }
}
