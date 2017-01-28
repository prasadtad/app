using Microsoft.Extensions.DependencyInjection;

namespace RecipeShelf.Lambda
{
    public static class Setup
    {
        public static IServiceCollection AddLambda(this IServiceCollection services)
        {
            return services.AddSingleton<UpdateIngredientRecords>()
                           .AddSingleton<UpdateRecipeRecords>()
                           .AddSingleton<UpdateIngredientRecordsFull>()
                           .AddSingleton<UpdateRecipeRecordsFull>();
        }
    }
}
