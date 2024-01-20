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

            var processor = client.CreateProcessor(_topicName, _subscriptionName, new ServiceBusProcessorOptions());
            processor.ProcessMessageAsync += ProcessMessageAsync;
            processor.ProcessErrorAsync += ProcessErrorAsync;

            await processor.StartProcessingAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(10000, stoppingToken);
                Console.WriteLine("Waiting for message...");
            }

            await processor.StopProcessingAsync();
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            try
            {
                string messageBody = args.Message.Body.ToString();
                _logger.LogInformation($"Received message: {messageBody}");
                if (!IsValidMessage(messageBody,out string projectlanguage, out string scanid, out Guid userid))
                {
                    Console.WriteLine("Invalid message format or missing arguments.");
                    await args.AbandonMessageAsync(args.Message);
                    return;
                }
                dynamic parsedMessage = JsonConvert.DeserializeObject(messageBody);
                string scanId = parsedMessage.scanid;
                Guid userId = parsedMessage.userid;
                await DependencyCheck(scanId, userId);

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while processing the message: {ex.Message}");

                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError($"Exception occurred while receiving message: {args.Exception.Message}");
            return Task.CompletedTask;
        }
        private bool IsValidMessage(string messageBody,out string projectlanguage, out string scanId, out Guid userId)
        {
            try
            {
                dynamic parsedMessage = JsonConvert.DeserializeObject(messageBody);

                scanId = parsedMessage.scanid;
                userId = parsedMessage.userid;
                projectlanguage = parsedMessage.projectlanguage;

                if (string.IsNullOrEmpty(scanId) || userId == Guid.Empty || projectlanguage != "c#")
                {
                    return false;
                }

                return true;
            }
            catch
            {
                projectlanguage = null;
                scanId = null;
                userId = Guid.Empty;
                return false;

            }
        }
        public async Task<bool> DependencyCheck(string scanId, Guid userId)
        {
            try
            {
                // Download file
                await _reportRepository.DownloadAsyncInstantDownload(scanId, userId.ToString());

                // Execute dependencyscan & unzip folder
                var foldername = await _dependencyScanService.UnzipFolder(scanId);

                //Remove .zip extension
                foldername.Replace(".zip", "");

                //Execute scan with JSON output
                await _dependencyScanService.ExecuteDependencyScan(foldername, "JSON");


                await _dependencyScanService.ExecuteDependencyScan(foldername, "HTML");
                Console.WriteLine("[INFO] Dependency Scan Successfull Executed");


                // Upload report to blob for later inspection
                await _reportRepository.UploadHtmlFileToBlobAsync(scanId, userId.ToString());
                Console.WriteLine("[INFO] Report Uploaded");
                //Make a new scan
                var dependencyScanId = await _sqlResultsStorageRepository.CreateScan(scanId.Replace(".zip", ""), userId);
                //Get the Id and use it for scanId in the DependencyCheckResults created/insertion
                Console.WriteLine("[INFO] Dependency Scan Created");

                // Get vulnerabilities
                var dependencyCheckResults = _extractJson.ExtractJson(scanId, dependencyScanId);
                Console.WriteLine("[INFO] Dependency Scan Successfull Executed");
                Console.WriteLine("[INFO] JSON Successfully Extracted");

                //store results
                await _sqlResultsStorageRepository.InsertDependencyInfosIntoDatabase(dependencyScanId, dependencyCheckResults);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
