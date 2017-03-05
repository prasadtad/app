using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeShelf.Data.Server.Proxies;

namespace RecipeShelf.Data.Server
{
    public static class Setup
    {
        public static IServiceCollection AddServerData(this IServiceCollection services, IConfigurationSection recipeshelfConfiguration)
        {
            services.Configure<DataServerSettings>(recipeshelfConfiguration.GetSection("DataServer"));
            return services.AddSingleton<IMarkdownProxy, LocalMarkdownProxy>()
                           .AddSingleton<IDistributedQueueProxy, SQSQueueProxy>();
        }
    }
}
