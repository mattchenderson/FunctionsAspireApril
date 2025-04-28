using System;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionsAspireApril
{
    public class MyQueueTrigger
    {
        private readonly ILogger<MyQueueTrigger> _logger;

        public MyQueueTrigger(ILogger<MyQueueTrigger> logger)
        {
            _logger = logger;
        }

        [Function(nameof(MyQueueTrigger))]
        public void Run([QueueTrigger("myqueue-items", Connection = "MyQueueConnection")] QueueMessage message)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");
        }
    }
}
