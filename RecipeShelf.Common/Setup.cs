using Microsoft.Extensions.DependencyInjection;
using RecipeShelf.Common.Proxies;

namespace RecipeShelf.Common
{
    public static class Setup
    {
        public static IServiceCollection AddCommon(this IServiceCollection services)
        {
            return Settings.FileProxyType == FileProxyTypes.Local ? services.AddSingleton<IFileProxy, LocalFileProxy>() :
                                                                        services.AddSingleton<IFileProxy, S3FileProxy>();
        }
    }
}
