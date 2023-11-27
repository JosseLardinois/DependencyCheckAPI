using Azure.Messaging.ServiceBus;
using DependencyCheckAPI.DTO;
using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.Models;
using Newtonsoft.Json;

namespace BackgroundTasks.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IDependencyScanService _dependencyScanService;
        private readonly IExtractJsonService _extractJson;
        private readonly IReportRepository _reportRepository;
        private readonly ILogger<Worker> _logger;
        private readonly ISQLResultsStorageRepository _sqlResultsStorageRepository;
        private readonly string _serviceBusConnectionString;
        private readonly string _topicName;
        private readonly string _subscriptionName;
        private readonly Guid _scanId;

        public Worker(ILogger<Worker> logger, IDependencyScanService dependencyScanService, IExtractJsonService extractJson, ISQLResultsStorageRepository sqlResultsStorageRepository, IReportRepository reportRepository)
        {
            _logger = logger;
            _dependencyScanService = dependencyScanService;
            _extractJson = extractJson;


            _serviceBusConnectionString = Environment.GetEnvironmentVariable("DCServiceBusConnection");
            _topicName = Environment.GetEnvironmentVariable("DCTopicName");
            _subscriptionName = Environment.GetEnvironmentVariable("DCSubscriptionName");
            _sqlResultsStorageRepository = sqlResultsStorageRepository;
            _reportRepository = reportRepository;
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
                await Task.Delay(10000, stoppingToken);
                Console.WriteLine("Waiting for message...");
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
                if (!IsValidMessage(messageBody,out string projectlanguage, out string filename, out Guid userid))
                {
                    Console.WriteLine("Invalid message format or missing arguments.");
                    await args.AbandonMessageAsync(args.Message);
                    return;
                }
                dynamic parsedMessage = JsonConvert.DeserializeObject(messageBody);
                string fileName = parsedMessage.filename;
                Guid userId = parsedMessage.userid;
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
        //use out string for passing multiple strings
        private bool IsValidMessage(string messageBody,out string projectlanguage, out string filename, out Guid userId)
        {
            try
            {
                dynamic parsedMessage = JsonConvert.DeserializeObject(messageBody);

                filename = parsedMessage.filename;
                userId = parsedMessage.userid;
                projectlanguage = parsedMessage.projectlanguage;

                // Validate if filename and userId exist in the parsed message
                if (string.IsNullOrEmpty(filename) || userId == Guid.Empty || projectlanguage != "c#")
                {
                    return false;
                }

                return true;
            }
            catch
            {
                projectlanguage = null;
                filename = null;
                userId = Guid.Empty;
                return false;

            }
        }
        public async Task<string> DependencyCheck(string filename, Guid userId)
        {
            try
            {
                // Download file
                await _reportRepository.DownloadAsyncInstantDownload(filename, userId.ToString());

                // Execute dependencyscan
                await _dependencyScanService.UnzipFolder(filename);
                Console.WriteLine("[INFO] Dependency Scan Successfull Executed");


                // Upload report to blob for later inspection
                await _reportRepository.UploadHtmlFileToBlobAsync(filename, userId.ToString());
                Console.WriteLine("[INFO] Report Uploaded");
                //Make a new scan
                var scanId = await _sqlResultsStorageRepository.CreateScan(filename, userId);
                //Get the Id and use it for scanId in the DependencyCheckResults created/insertion
                Console.WriteLine("[INFO] Dependency Scan Created");

                // Get vulnerabilities
                var dependencyCheckResults = _extractJson.ExtractJson(filename, scanId);
                Console.WriteLine("[INFO] Dependency Scan Successfull Executed");
                Console.WriteLine("[INFO] JSON Successfully Extracted");
                //store results

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
