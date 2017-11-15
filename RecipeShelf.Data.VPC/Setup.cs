using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeShelf.Data.VPC.Proxies;

namespace RecipeShelf.Data.VPC
{
    public static class Setup
    {
        public static IServiceCollection AddVPCData(this IServiceCollection services, IConfigurationSection recipeshelfConfiguration)
        {
            services.Configure<DataVPCSettings>(recipeshelfConfiguration.GetSection("DataVPC"));
            return services.AddSingleton<ICacheProxy, RedisProxy>()
                           .AddSingleton<RecipeCache>();
        }
    }
}
