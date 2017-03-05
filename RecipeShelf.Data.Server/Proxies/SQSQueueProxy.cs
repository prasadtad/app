using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RecipeShelf.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeShelf.Data.Server.Proxies
{
    public class SQSQueueProxy : IDistributedQueueProxy, IDisposable
    {
        private readonly ILogger<SQSQueueProxy> _logger;

        private readonly DataServerSettings _settings;

        private AmazonSQSClient _client;

        public SQSQueueProxy(ILogger<SQSQueueProxy> logger, IOptions<DataServerSettings> optionsAccessor)
        {
            _logger = logger;
            _settings = optionsAccessor.Value;
            _client = _settings.UseLocalQueue ? new AmazonSQSClient(new BasicAWSCredentials("Local", "Local"), new AmazonSQSConfig { ServiceURL = "http://localhost:9324" }) : new AmazonSQSClient();
        }

        public async Task ProcessMessagesAsync(string queueName, Func<IEnumerable<DistributedQueueMessage>, Task> messagesProcessorAsync)
        {
            var stopWatch = new Stopwatch();
            var queueUrl = _settings.SQSUrlPrefix + queueName;
            while (true)
            {
                var response = await ReceiveMessageFromQueueAsync(queueUrl);
                if (response.Messages.Count == 0)
                {
                    _logger.LogInformation("No messages on {QueueName}, sleeping for a minute", queueName);
                    await Task.Delay(60000);
                    continue;
                }
                _logger.LogInformation("Processing {Count} messages on {QueueName}", response.Messages.Count, queueName);
                stopWatch.Restart();
                var messages = response.Messages.Select(m => new DistributedQueueMessage(m)).ToArray();
                await messagesProcessorAsync(messages);
                foreach (var m in messages)
                {
                    if (m.Processed == null) throw new Exception("Message processing status not set unexpectedly.");
                    if (m.Processed.Value)
                    {
                        _logger.LogDebug("Processed {Message} on {QueueName}", JsonConvert.SerializeObject(m), queueName);
                        await _client.DeleteMessageAsync(queueUrl, m.ReceiptHandle);
                    }
                    else
                        _logger.LogError("Couldn't process {Message} on {QueueName}", JsonConvert.SerializeObject(m), queueName);
                }
                _logger.LogInformation("Processing {Count} messages on {QueueName} took {Duration}", response.Messages.Count, queueName, stopWatch.Elapsed.Describe());
            }
        }

        private async Task<ReceiveMessageResponse> ReceiveMessageFromQueueAsync(string queueUrl)
        {
            ReceiveMessageResponse response = null;
            while (response == null)
            {
                try
                {
                    response = await _client.ReceiveMessageAsync(queueUrl);
                }
                catch (OverLimitException)
                {
                    _logger.LogError("Went over limit receiving messages from {QueueUrl}", queueUrl);
                    await _client.PurgeQueueAsync(queueUrl);
                    _logger.LogInformation("Purging {QueueUrl} and waiting for 60 seconds before trying again", queueUrl);
                    await Task.Delay(60000);
                }
            }
            return response;
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }
        }
    }
}
