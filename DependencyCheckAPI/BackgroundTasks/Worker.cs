using Azure.Messaging.ServiceBus;
using DependencyCheckAPI.Dto;
using DependencyCheckAPI.Interfaces;
using Newtonsoft.Json;

namespace BackgroundTasks.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IDependencyScanRepository _dependencyScanRepository;
        private readonly IExtractJson _extractJson;
        private readonly IAzureFileRepository _azureRepository;
        private readonly ILogger<Worker> _logger;
        private readonly string _serviceBusConnectionString;
        private readonly string _topicName;
        private readonly string _subscriptionName;

        public Worker(ILogger<Worker> logger, IDependencyScanRepository dependencyScanRepository, IExtractJson extractJson, IAzureFileRepository azureRepository)
        {
            _logger = logger;
            _dependencyScanRepository = dependencyScanRepository;
            _extractJson = extractJson;
            _azureRepository = azureRepository;

            // Replace these values with your actual Azure Service Bus connection string, topic name, and subscription name.

            _serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnection");
            _topicName = Environment.GetEnvironmentVariable("TopicName");
            _subscriptionName = Environment.GetEnvironmentVariable("SubscriptionName");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using var client = new ServiceBusClient(_serviceBusConnectionString);

            // Create a processor to process messages from the topic subscription
            var processor = client.CreateProcessor(_topicName, _subscriptionName, new ServiceBusProcessorOptions());

            // Add handlers to process messages and errors
            processor.ProcessMessageAsync += ProcessMessageAsync;
            processor.ProcessErrorAsync += ProcessErrorAsync;

            // Start processing
            await processor.StartProcessingAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            // Stop processing
            await processor.StopProcessingAsync();
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            try
            {
                // Process the message
                string messageBody = args.Message.Body.ToString();
                _logger.LogInformation($"Received message: {messageBody}");
                if (!IsValidMessage(messageBody, out string filename, out string userid))
                {
                    Console.WriteLine("Invalid message format or missing arguments.");
                    await args.AbandonMessageAsync(args.Message);
                    return;
                }
                dynamic parsedMessage = JsonConvert.DeserializeObject(messageBody);
                string fileName = parsedMessage.filename;
                string userId = parsedMessage.userid;
                await DependencyCheck(fileName, userId);

                // Complete the message to remove it from the subscription
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur while processing the message
                _logger.LogError($"Error occurred while processing the message: {ex.Message}");

                // Abandon the message to let the Service Bus retry processing it
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            // Handle any exceptions that occur during the message handler execution
            _logger.LogError($"Exception occurred while receiving message: {args.Exception.Message}");
            return Task.CompletedTask;
        }

        private bool IsValidMessage(string messageBody, out string filename, out string userId)
        {
            try
            {
                dynamic parsedMessage = JsonConvert.DeserializeObject(messageBody);

                filename = parsedMessage.filename;
                userId = parsedMessage.userid;

                // Validate if filename and userId exist in the parsed message
                if (string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(userId))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                filename = null;
                userId = null;
                return false;

            }
        }
        public async Task<string> DependencyCheck(string filename, string userId)
        {
            // Check if file exists
            if (!await _azureRepository.DoesFileExistInBlob(filename, userId))
            {

                return $"File {filename} not found in container.";
            }
            try
            {
                // Download file
                BlobDto? file = await _azureRepository.GetBlobFile(filename, userId);
                if (file == null)
                {
                    // Was not, return error message to client
                    return $"File {filename} could not be downloaded.";
                }

                // Execute dependencyscan
                await _dependencyScanRepository.UnzipFolder(filename);
                Console.WriteLine("[INFO] Dependency Scan Successfull Executed");


                // Upload report to blob for later inspection
                await _azureRepository.UploadHtmlReport(filename, userId);

                //Make a new project with user
                _extractJson.MakeNewProject(userId, filename);

                // Store main vulnerabilities
                _extractJson.ExtractJson(filename);

                return "ok";
            }
            catch (Exception ex)
            {
                // Handle other exceptions here if needed, and send an appropriate response to the client.
                // You can also log the error if needed.
                return $"An error occurred: {ex.Message}";
            }
        }
    }
}
