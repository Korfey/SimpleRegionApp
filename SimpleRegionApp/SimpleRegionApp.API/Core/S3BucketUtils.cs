using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.IO;

namespace SimpleRegionApp.API.Core;

public class S3BucketUtils(string bucketName, IAmazonS3 s3Client)
{
    private readonly string keyPrefix = "/Images/";
    public async Task<Stream> DownloadByName(string name)
    {
        return await s3Client.GetObjectStreamAsync(bucketName, keyPrefix + name, null);
    }

    public async Task<bool> FileExists(string key)
    {
        try
        {
            await s3Client.GetObjectMetadataAsync(bucketName, keyPrefix + key);
        }
        catch (AmazonS3Exception ex)
        {
            if (string.Equals(ex.ErrorCode, "NotFound"))
                return false;
        }

        return true;
    }

    public async Task UploadImage(IFormFile file)
    {
        using var stream = file.OpenReadStream();

        await s3Client.UploadObjectFromStreamAsync(
            bucketName,
            keyPrefix + file.FileName,
            stream,
            null
        );
    }

    public async Task DeleteImageByName(string key)
    {
        try
        {
            await s3Client.DeleteObjectAsync(bucketName, keyPrefix + key);
        }
        catch (AmazonS3Exception ex)
        {
            if (string.Equals(ex.ErrorCode, "NotFound"))
                throw new Exception("File not found");
        }
    }
}
