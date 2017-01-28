using Microsoft.Extensions.DependencyInjection;

namespace RecipeShelf.NoSql
{
    public static class Setup
    {
        public static IServiceCollection AddNoSql(this IServiceCollection services)
        {
            return services.AddSingleton<INoSqlDbProxy, DynamoDbProxy>(c => new DynamoDbProxy());
        }
    }
}
