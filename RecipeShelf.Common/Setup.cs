using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeShelf.Common.Proxies;

namespace RecipeShelf.Common
{
    public static class Setup
    {
        public static IServiceCollection AddCommon(this IServiceCollection services, IConfigurationSection recipeshelfConfiguration)
        {
            var commonSection = recipeshelfConfiguration.GetSection("Common");
            services.Configure<CommonSettings>(commonSection);
            services.AddSingleton<INoSqlDbProxy, DynamoDbProxy>();
            return commonSection.GetValue<FileProxyTypes>("FileProxyType") == FileProxyTypes.Local ? services.AddSingleton<IFileProxy, LocalFileProxy>() :
                                                                    services.AddSingleton<IFileProxy, S3FileProxy>();
        }
    }
}
