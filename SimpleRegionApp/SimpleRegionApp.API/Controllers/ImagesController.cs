using Microsoft.AspNetCore.Mvc;
using Amazon.Util;
using static System.Net.Mime.MediaTypeNames;
using SimpleRegionApp.API.Models.DTO;
using SimpleRegionApp.API.Core;
using SimpleRegionApp.API.Data;
using Microsoft.EntityFrameworkCore;
using SimpleRegionApp.API.Models.Mappers;
using SimpleRegionApp.Models;
using Amazon.S3;

[Route("api/[controller]")]
[ApiController]
public class ImagesController : ControllerBase
{
    private readonly string s3Path;
    private readonly SimpleDbContext dbContext;
    private readonly IAmazonS3 s3Client;
    private readonly S3BucketUtils s3BucketUtils;

    public ImagesController(IConfiguration configuration, SimpleDbContext dbContext, IAmazonS3 s3Client)
    {
        s3Path = configuration["s3Bucket"] ?? throw new Exception("No S3Path found in appsettings");

        this.s3Client = s3Client;
        this.dbContext = dbContext;
        s3BucketUtils = new S3BucketUtils(s3Path, this.s3Client);

        dbContext.Database.EnsureCreated();
    }

    /// <summary>
    /// Download an image by name.
    /// </summary>
    /// <param name="name">Image name in s3 bucket.</param>
    /// <returns>Image file.</returns>
    [HttpGet("download/{name}")]
    public async Task<IActionResult> DownloadImage(string name)
    {
        if (!await s3BucketUtils.FileExists(name))
            return NotFound("Image not found.");
        
        var stream = await s3BucketUtils.DownloadByName(name);
        return File(stream, Application.Octet, name);
    }

    /// <summary>
    /// Show metadata for the existing image by name.
    /// </summary>
    /// <param name="name">Image name.</param>
    /// <returns>Metadata of the image.</returns>
    [HttpGet("metadata/{name}")]
    public async Task<IActionResult> GetImageMetadata(string name)
    {
        var metadata = await dbContext.Images
            .FirstOrDefaultAsync(img => img.Name == name);

        return metadata is null
            ? NotFound("No image with such name found")
            : Ok(metadata.ToMetaDataResponse());
    }

    /// <summary>
    /// Show metadata for a random image.
    /// </summary>
    /// <returns>Metadata of a random image.</returns>
    [HttpGet("metadata/random")]
    public async Task<IActionResult> GetRandomImageMetadata()
    {
        var metadata = await dbContext.Images
            .OrderBy(x => Guid.NewGuid())
            .FirstOrDefaultAsync();

        return metadata is null
            ? NotFound("No images have been uploaded yet.")
            : Ok(metadata.ToMetaDataResponse());
    }

    /// <summary>
    /// Upload an image.
    /// </summary>
    /// <param name="file">Image file to upload.</param>
    /// <returns>Result of the upload operation.</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        if(await s3BucketUtils.FileExists(file.FileName))
        {
            return BadRequest("File with such name already exists.");
        }

        await s3BucketUtils.UploadImage(file);

        var metadata = new Metadata
        {
            Name = file.FileName,
            ImageSize = file.Length,
            FileExtension = Path.GetExtension(file.FileName),
            LastUpdate = DateTime.UtcNow
        };

        dbContext.Images.Add(metadata);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(UploadImage), metadata);
    }

    /// <summary>
    /// Delete an image by name.
    /// </summary>
    /// <param name="name">Image name.</param>
    /// <returns>Result of the delete operation.</returns>
    [HttpDelete("{name}")]
    public async Task<IActionResult> DeleteImage(string name)
    {
        if (!await s3BucketUtils.FileExists(name))
            return NotFound("Image not found.");

        await s3BucketUtils.DeleteImageByName(name);

        dbContext.Remove(dbContext.Images.First(img => img.Name == name));
        await dbContext.SaveChangesAsync();

        return Ok(new { Message = "Image deleted successfully."});
    }
}
