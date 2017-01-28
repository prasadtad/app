using Microsoft.Extensions.DependencyInjection;

namespace RecipeShelf.Site
{
    public static class Setup
    {
        public static IServiceCollection AddSite(this IServiceCollection services)
        {
            return services.AddSingleton<IMarkdownProxy, LocalMarkdownProxy>();
        }
    }
}
