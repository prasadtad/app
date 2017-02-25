using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeShelf.Data.Server.Proxies
{
    public class DistributedQueueMessage
    {
        public readonly string Body;

        public readonly IDictionary<string, string> Attributes;

        internal readonly string ReceiptHandle;

        public bool? Processed;

        internal DistributedQueueMessage(Message message)
        {
            Body = message.Body;
            Attributes = message.MessageAttributes.ToDictionary(kv => kv.Key, kv => kv.Value.StringValue);
            ReceiptHandle = message.ReceiptHandle;
        }
    }

    public interface IDistributedQueueProxy
    {
        Task ProcessMessagesAsync(string queueName, Func<IEnumerable<DistributedQueueMessage>, Task> messagesProcessorAsync);
    }

}
