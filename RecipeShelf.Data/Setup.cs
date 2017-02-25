using Microsoft.Extensions.DependencyInjection;
using RecipeShelf.Data.Proxies;

namespace RecipeShelf.Data
{
    public static class Setup
    {
        public static IServiceCollection AddData(this IServiceCollection services)
        {
            return services.AddSingleton<INoSqlDbProxy, DynamoDbProxy>(c => new DynamoDbProxy());
        }
    }
}
