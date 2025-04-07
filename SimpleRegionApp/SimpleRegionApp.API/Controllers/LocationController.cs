using Microsoft.AspNetCore.Mvc;
using Amazon.Util;

[Route("api/[controller]")]
[ApiController]
public class LocationController : ControllerBase
{
    /// <summary>
    /// Method to access current EC2 instance region and availability zone
    /// </summary>
    /// <returns> Region and AZ </returns>
    [HttpGet]
    public IActionResult GetRegionAndAz()
    {
        string region = EC2InstanceMetadata.Region.DisplayName;
        string az = EC2InstanceMetadata.AvailabilityZone;

        return Ok(new { Region = region, AvailabilityZone = az });
    }
}