
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using SimpleRegionApp.API.Models.DTO;
using System.Text.Json;
using System.Threading;

namespace SimpleRegionApp.API;

public class SqsPollingService(
    IConfiguration config,
    IAmazonSQS sqsClient,
    IAmazonSimpleNotificationService snsClient) : BackgroundService
{
    private readonly string SqsUrl = config["SqsUrl"] ?? throw new Exception("No SqsUrl found in appsettings");
    private readonly string SnsArn = config["SnsArn"] ?? throw new Exception("No SnsArn found in appsettings");
    private readonly string albEndpoint = Environment.GetEnvironmentVariable("ALB_ENDPOINT") ?? throw new Exception("ALB_ENDPOINT not set");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var receiveRequest = new ReceiveMessageRequest
            {
                QueueUrl = SqsUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 20
            };

            var response = await sqsClient.ReceiveMessageAsync(receiveRequest, stoppingToken);

            if (response.Messages is null)
            {
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                continue;
            }

            foreach (var message in response.Messages)
            {
                try
                {
                    var metadata = JsonSerializer.Deserialize<MetaDataResponse>(message.Body);
                    string notificationMessage = BuildResponse(metadata);
                    
                    var publishRequest = BuildRequest(metadata, notificationMessage);

                    await snsClient.PublishAsync(publishRequest, stoppingToken);

                    await sqsClient.DeleteMessageAsync(SqsUrl, message.ReceiptHandle, stoppingToken);
                }
                catch (Exception)
                {
                    // Do nothing
                }
            }
        }
    }

    private PublishRequest BuildRequest(MetaDataResponse? metadata, string notificationMessage)
    {
        return new PublishRequest
        {
            TopicArn = SnsArn,
            Message = notificationMessage,
            MessageAttributes = new Dictionary<string, Amazon.SimpleNotificationService.Model.MessageAttributeValue>
            {
                {
                    "imageExtension", new Amazon.SimpleNotificationService.Model.MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = metadata.FileExtension
                    }
                }
            }
        };
    }

    private string BuildResponse(MetaDataResponse? metadata)
    {
        return
            $"""
            New image uploaded to S3 bucket.
                Image Name: {metadata.Name}
                Image Size: {metadata.ImageSize} bytes
                Image Extension: {metadata.FileExtension}
                Image Upload Date: {metadata.LastUpdate}
            Download link: {albEndpoint}/api/images/download/{metadata.Name}
            """;
    }
}
