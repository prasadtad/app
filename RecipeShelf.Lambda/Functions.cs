using Amazon.Lambda.Core;
using System;
using Microsoft.Extensions.DependencyInjection;
using RecipeShelf.Common;
using System.Threading.Tasks;
using Amazon.Lambda.S3Events;
using RecipeShelf.Data;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace RecipeShelf.Lambda
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
                                    .AddData()
                                    .AddLambda()
                                    .BuildServiceProvider();
        }

        public async Task UpdateIngredientRecords(S3Event e, ILambdaContext context)
        {
            var updateIngredientRecords = _serviceProvider.GetService<UpdateIngredientRecords>();
            foreach (var record in e.Records)
            {
                _logger.Debug("UpdateIngredientRecords", $"{record.S3.Bucket.Name} - {record.S3.Object.Key}");
                await updateIngredientRecords.ExecuteAsync(record.S3.Object.Key);
            }
        }

        public async Task UpdateRecipeRecords(S3Event e, ILambdaContext context)
        {
            var updateRecipeRecords = _serviceProvider.GetService<UpdateRecipeRecords>();
            foreach (var record in e.Records)
            {
                _logger.Debug("UpdateRecipeRecords", $"{record.S3.Bucket.Name} - {record.S3.Object.Key}");
                await updateRecipeRecords.ExecuteAsync(record.S3.Object.Key);
            }
        }

        public async Task UpdateIngredientRecordsFull(ILambdaContext context)
        {
            _logger.Debug("UpdateIngredientRecordsFull", "started");
            await _serviceProvider.GetService<UpdateIngredientRecordsFull>().ExecuteAsync();
        }

        public async Task UpdateRecipeRecordsFull(ILambdaContext context)
        {
            _logger.Debug("UpdateRecipeRecordsFull", "started");
            await _serviceProvider.GetService<UpdateRecipeRecordsFull>().ExecuteAsync();
        }
    }
}