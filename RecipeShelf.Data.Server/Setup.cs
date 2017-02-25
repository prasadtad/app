using Microsoft.Extensions.DependencyInjection;
using RecipeShelf.Data.Server.Proxies;

namespace RecipeShelf.Data.Server
{
    public static class Setup
    {
        public static IServiceCollection AddServerData(this IServiceCollection services)
        {
            return services.AddSingleton<IMarkdownProxy, LocalMarkdownProxy>()
                           .AddSingleton<IDistributedQueueProxy, SQSQueueProxy>();
        }
    }
}
