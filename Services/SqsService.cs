using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApiSQSIntegration.Services
{
    public class SqsService
    {
        private readonly AmazonSQSClient _sqsClient;
        private Dictionary<string, string> _queueUrls = new Dictionary<string, string>();

        public SqsService(IConfiguration configuration)
        {
            var awsOptions = configuration.GetSection("AWS");
            var accessKeyId = awsOptions["AccessKeyId"];
            var secretAccessKey = awsOptions["SecretAccessKey"];
            var region = awsOptions["Region"];
            // Initialize the Amazon SQS client with the credentials and region
            var config = new AmazonSQSConfig { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region) };
            _sqsClient = new AmazonSQSClient(accessKeyId, secretAccessKey, config);
        }

        public async Task<string> GetOrCreateQueueUrlAsync(string queueName)
        {
            // Check if the queue URL is already stored
            if (!_queueUrls.ContainsKey(queueName))
            {
                try
                {
                    var getQueueUrlResponse = await _sqsClient.GetQueueUrlAsync(new GetQueueUrlRequest { QueueName = queueName });
                    _queueUrls[queueName] = getQueueUrlResponse.QueueUrl;
                }
                catch (QueueDoesNotExistException)
                {
                    // If the queue does not exist, create it
                    var createQueueResponse = await _sqsClient.CreateQueueAsync(new CreateQueueRequest { QueueName = queueName });
                    _queueUrls[queueName] = createQueueResponse.QueueUrl;
                }
            }
            return _queueUrls[queueName];
        }

        public async Task SendMessageAsync(string queueName, string messageBody)
        {
            var queueUrl = await GetOrCreateQueueUrlAsync(queueName);
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody
            };
            await _sqsClient.SendMessageAsync(sendMessageRequest);
        }

        public async Task<List<Message>> ReceiveMessagesAsync(string queueName)
        {
            var queueUrl = await GetOrCreateQueueUrlAsync(queueName);
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 20
            };
            var response = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest);
            return response.Messages;
        }

        public async Task DeleteMessageAsync(string queueName, string receiptHandle)
        {
            var queueUrl = await GetOrCreateQueueUrlAsync(queueName);
            var deleteMessageRequest = new DeleteMessageRequest
            {
                QueueUrl = queueUrl,
                ReceiptHandle = receiptHandle
            };
            await _sqsClient.DeleteMessageAsync(deleteMessageRequest);
        }
    }

    // Usage
    // var sqsService = new SqsService("your-access-key-id", "your-secret-access-key", "us-west-2");
    // await sqsService.SendMessageAsync("MyQueue", "Hello, SQS!");
    // var messages = await sqsService.ReceiveMessagesAsync("MyQueue");
    // foreach (var message in messages)
    // {
    //     Console.WriteLine($"Received message: {message.Body}");
    //     await sqsService.DeleteMessageAsync("MyQueue", message.ReceiptHandle);
    // }

}