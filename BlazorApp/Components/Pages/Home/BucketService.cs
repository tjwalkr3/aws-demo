using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

namespace BlazorApp.Components.Pages.Home;

public sealed class BucketService : IBucketService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public BucketService(IConfiguration configuration)
    {
        _bucketName =
            configuration["S3_BUCKET_NAME"]
            ?? throw new InvalidOperationException("S3_BUCKET_NAME is not configured.");

        var regionName = configuration["AWS_REGION"] ?? "us-east-1";
        var region = RegionEndpoint.GetBySystemName(regionName);
        var accessKey = configuration["AWS_ACCESS_KEY_ID"];
        var secretKey = configuration["AWS_SECRET_ACCESS_KEY"];

        _s3Client =
            !string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretKey)
                ? new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), region)
                : new AmazonS3Client(region);
    }

    public async Task<IReadOnlyList<BucketFileDto>> ListFilesAsync(
        CancellationToken cancellationToken = default
    )
    {
        var files = new List<BucketFileDto>();
        string? continuationToken = null;

        do
        {
            var response = await _s3Client.ListObjectsV2Async(
                new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    ContinuationToken = continuationToken,
                },
                cancellationToken
            );

            files.AddRange(
                response.S3Objects.Select(file => new BucketFileDto
                {
                    Key = file.Key,
                    FileName = Path.GetFileName(file.Key),
                    LastModifiedUtc = file.LastModified.HasValue
                        ? new DateTimeOffset(file.LastModified.Value)
                        : DateTimeOffset.MinValue,
                })
            );

            continuationToken =
                response.IsTruncated == true ? response.NextContinuationToken : null;
        } while (!string.IsNullOrWhiteSpace(continuationToken));

        return files.OrderByDescending(file => file.LastModifiedUtc).ToList();
    }

    public async Task UploadFileAsync(
        string fileName,
        Stream content,
        string? contentType,
        CancellationToken cancellationToken = default
    )
    {
        if (content.CanSeek)
        {
            content.Position = 0;
        }

        await _s3Client.PutObjectAsync(
            new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                InputStream = content,
                ContentType = string.IsNullOrWhiteSpace(contentType)
                    ? "application/octet-stream"
                    : contentType,
            },
            cancellationToken
        );
    }

    public async Task DeleteFileAsync(string key, CancellationToken cancellationToken = default)
    {
        await _s3Client.DeleteObjectAsync(
            new DeleteObjectRequest { BucketName = _bucketName, Key = key },
            cancellationToken
        );
    }

    public Task<string> GetDownloadUrlAsync(
        string key,
        CancellationToken cancellationToken = default
    )
    {
        var url = _s3Client.GetPreSignedURL(
            new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(15),
            }
        );

        return Task.FromResult(url);
    }
}
