using System.Net.Mime;
using Amazon.S3;
using Amazon.S3.Model;

using GlovoBackEnd.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;


namespace Core.Services;

public class MinioImageService(IAmazonS3 s3Client, IConfiguration  config) : IMinioImageService
{
    private readonly string _bucketName = config["MinIO:BucketName"]!;
    private readonly string _endpoint = config["MinIO:PublicEndpoint"]!;


    public async Task<string> UploadImageAsync(IFormFile file, CancellationToken ct)
    {
        var key = $"images/{Guid.NewGuid()}.webp"; 

        using var inputStream = file.OpenReadStream();
        using var image = await Image.LoadAsync(inputStream, ct);    
        image.Mutate(img => img.Resize(new ResizeOptions
        {
            Size = new Size(600, 600),
            Mode = ResizeMode.Max,
        }));

        using var outputStream = new MemoryStream();
        await image.SaveAsWebpAsync(outputStream, ct); 
        outputStream.Position = 0;

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = outputStream,
            ContentType = "image/webp" 
        };

        await s3Client.PutObjectAsync(request, ct);
        return $"{_endpoint}/{_bucketName}/{key}";
    }

    public async Task<string> UploadImageFromUrlAsync(string url, CancellationToken ct)
    {
        using var httpClient = new HttpClient();
        var bytes = await httpClient.GetByteArrayAsync(url, ct);

        using var image = Image.Load(bytes);
        image.Mutate(img => img.Resize(new ResizeOptions
        {
            Size = new Size(600, 600),
            Mode = ResizeMode.Max,
        }));

        using var outputStream = new MemoryStream();
        await image.SaveAsWebpAsync(outputStream, ct); 
        outputStream.Position = 0;

        var key = $"images/{Guid.NewGuid()}.webp"; 

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = outputStream,
            ContentType = "image/webp" 
        };

        await s3Client.PutObjectAsync(request, ct);
        return $"{_endpoint}/{_bucketName}/{key}";
    }
    public async Task DeleteImageAsync(string path, CancellationToken ct)
    {
        var key = path.Replace($"{_endpoint}/{_bucketName}/", "");

        await s3Client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        }, ct);
    }
    public async Task<string> UpdateImageAsync(string oldPath, IFormFile newFile, CancellationToken ct)
    {
        await DeleteImageAsync(oldPath, ct);
        return await UploadImageAsync(newFile, ct);
    }
    
}