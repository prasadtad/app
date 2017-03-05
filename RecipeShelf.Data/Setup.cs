using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeShelf.Data.Proxies;

namespace RecipeShelf.Data
{
    public static class Setup
    {
        public static IServiceCollection AddData(this IServiceCollection services, IConfigurationSection recipeshelfConfiguration)
        {
            services.Configure<DataSettings>(recipeshelfConfiguration.GetSection("Data"));
            return services.AddSingleton<INoSqlDbProxy, DynamoDbProxy>();
        }
    }
}
