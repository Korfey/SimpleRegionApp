using Amazon.SQS;
using SimpleRegionApp.API.Models.DTO;
using System.Text.Json;

namespace SimpleRegionApp.API.Core;

public class SqsUtils(IAmazonSQS amazonSQS, string queryUrl)
{
    public async Task SendImage(MetadataSqsDto metadata)
    {
        string body = JsonSerializer.Serialize(metadata);
        await amazonSQS.SendMessageAsync(queryUrl, body);
    }
}
