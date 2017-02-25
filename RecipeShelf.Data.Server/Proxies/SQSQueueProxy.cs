using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
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
        private readonly Logger<SQSQueueProxy> _logger = new Logger<SQSQueueProxy>();

        private AmazonSQSClient _client;

        public SQSQueueProxy()
        {
            _client = Settings.UseLocalQueue ? new AmazonSQSClient(new BasicAWSCredentials("Local", "Local"), new AmazonSQSConfig { ServiceURL = "http://localhost:9324" }) : new AmazonSQSClient();
        }

        public async Task ProcessMessagesAsync(string queueName, Func<IEnumerable<DistributedQueueMessage>, Task> messagesProcessorAsync)
        {
            var stopWatch = new Stopwatch();
            var queueUrl = Settings.SQSUrlPrefix + queueName;
            while (true)
            {
                var response = await ReceiveMessageFromQueueAsync(queueUrl);
                if (response.Messages.Count == 0)
                {
                    _logger.Information("ProcessMessages", $"No messages on {queueName}, sleeping for 2 seconds");
                    await Task.Delay(2000);
                    continue;
                }
                _logger.Information("ProcessMessages", $"Processing {response.Messages.Count} messages on {queueName}");
                stopWatch.Restart();
                var messages = response.Messages.Select(m => new DistributedQueueMessage(m)).ToArray();
                await messagesProcessorAsync(messages);
                foreach (var m in messages)
                {
                    if (m.Processed == null) throw new Exception("Message processing status not set unexpectedly.");
                    if (m.Processed.Value)
                    {
                        _logger.Error("ProcessMessages", $"Processed message {JsonConvert.SerializeObject(m)} on {queueName}");
                        await _client.DeleteMessageAsync(queueUrl, m.ReceiptHandle);                        
                    }
                    else
                        _logger.Error("ProcessMessages", $"Couldn't process message {JsonConvert.SerializeObject(m)} on {queueName}");
                }
                stopWatch.Stop();
                _logger.Information("ProcessMessages", $"Processing {response.Messages.Count} messages on {queueName} took {stopWatch.Elapsed}");
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
                    _logger.Error("ReceiveMessageFromQueue", $"Went over limit receiving messages from {queueUrl}");
                    await _client.PurgeQueueAsync(queueUrl);
                    _logger.Information("ReceiveMessageFromQueue", $"Purging {queueUrl} and waiting for 60 seconds before trying again");
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
