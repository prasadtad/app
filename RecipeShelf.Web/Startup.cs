using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RecipeShelf.Common;
using RecipeShelf.Data.VPC;

namespace RecipeShelf.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var recipeshelfConfiguration = Configuration.GetSection("RecipeShelf");
            // Add framework services.
            services.AddOptions()
                    .AddCommon(recipeshelfConfiguration)
                    .AddVPCData(recipeshelfConfiguration)
                    .AddMvc();
            AddWeb(services, recipeshelfConfiguration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var recipeshelfConfiguration = Configuration.GetSection("RecipeShelf");
            if (recipeshelfConfiguration.GetValue<bool>("LogToConsole"))
            {
                loggerFactory.AddConsole(Configuration.GetSection("Logging"));
                loggerFactory.AddDebug();
            }
            else
                loggerFactory.AddAWSProvider(Configuration.GetAWSLoggingConfigSection());

            app.UseMvc();
        }

        private void AddWeb(IServiceCollection services, IConfigurationSection recipeshelfConfiguration)
        {
            services.AddSingleton<IRecipeRepository, RecipeRepository>();
        }
    }
}
