using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.AspNetCore.Mvc;
using SimpleRegionApp.API.Core;

[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly IAmazonSimpleNotificationService snsClient;
    private readonly SnsUtils snsManager;
    public NotificationController(IAmazonSimpleNotificationService SnsClient, IConfiguration configuration)
    {
        snsClient = new AmazonSimpleNotificationServiceClient();
        snsManager = new SnsUtils(snsClient, configuration["SnsArn"] ?? throw new Exception("SnsArn not found"));
    }

    [HttpPost("subscribe/{email}")]
    public async Task<IActionResult> Subscribe(string email)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest("Email is required");

        var response = await snsManager.Subscribe(email);

        return Ok("Confirmation email sent.");
    }

    [HttpPost("unsubscribe/{email}")]
    public async Task<IActionResult> Unsubscribe(string email)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest("Email is required");

        var response = await snsManager.Unsubscribe(email);

        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            return BadRequest("Failed to unsubscribe.");

        return Ok("Unsubscribed successfully.");
    }

}
