using Amazon.Lambda.Core;
using Newtonsoft.Json;
using RecipeShelf.Cache.Models;
using System;
using Microsoft.Extensions.DependencyInjection;
using RecipeShelf.Common;
using RecipeShelf.Cache;
using System.Threading.Tasks;
using Amazon.Lambda.S3Events;
using System.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace RecipeShelf.VPCLambda
{
    public sealed class Functions
    {
        private static IServiceProvider _serviceProvider;
        private static Logger<Functions> _logger;

        static Functions()
        {
            _logger = new Logger<Functions>();
            _serviceProvider = new ServiceCollection()
                                    .AddCommon()
                                    .AddCaching()
                                    .AddVPCLambda()
                                    .BuildServiceProvider();
        }

        public string[] GetRecipesFromCache(RecipeFilter input, ILambdaContext context)
        {
            _logger.Debug("GetRecipesFromCache", JsonConvert.SerializeObject(input));
            return _serviceProvider.GetService<GetRecipesFromCache>().Execute(input);
        }

        public async Task UpdateIngredientCache(S3Event e, ILambdaContext context)
        {
            var updateIngredientCache = _serviceProvider.GetService<UpdateIngredientCache>();
            foreach (var record in e.Records)
            {
                _logger.Debug("UpdateIngredientCache", $"{record.S3.Bucket.Name} - {record.S3.Object.Key}");
                await updateIngredientCache.ExecuteAsync(record.S3.Object.Key);
            }
        }

        public async Task UpdateRecipeCache(S3Event e, ILambdaContext context)
        {
            var updateRecipeCache = _serviceProvider.GetService<UpdateRecipeCache>();
            foreach (var record in e.Records)
            {
                _logger.Debug("UpdateRecipeCache", $"{record.S3.Bucket.Name} - {record.S3.Object.Key}");
                await updateRecipeCache.ExecuteAsync(record.S3.Object.Key);
            }
        }

        public async Task UpdateIngredientCacheFull(ILambdaContext context)
        {
            _logger.Debug("UpdateIngredientCacheFull", "started");
            await _serviceProvider.GetService<UpdateIngredientCacheFull>().ExecuteAsync();
        }

        public async Task UpdateRecipeCacheFull(ILambdaContext context)
        {
            _logger.Debug("UpdateRecipeCacheFull", "started");
            await _serviceProvider.GetService<UpdateRecipeCacheFull>().ExecuteAsync();
        }
    }
}