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
    private const long MaxImageSizeBytes = 8 * 1024 * 1024;
    private readonly string _bucketName = config["MinIO:BucketName"]!;
    private readonly string _endpoint = config["MinIO:PublicEndpoint"]!.TrimEnd('/');

    public async Task<string> UploadImageAsync(IFormFile file, ImageUploadKind kind, CancellationToken ct)
    {
        ValidateFile(file);
        var (folder, size) = GetProfile(kind);
        var key = $"{folder}/{Guid.NewGuid()}.webp";

        using var inputStream = file.OpenReadStream();
        Image image;
        try
        {
            image = await Image.LoadAsync(inputStream, ct);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new ArgumentException("The selected file is not a valid image.", nameof(file), exception);
        }

        using (image)
        {
        ValidateDimensions(image);
        image.Mutate(img => img.Resize(new ResizeOptions
        {
            Size = size,
            Mode = ResizeMode.Crop,
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
    }

    public async Task<string> UploadImageFromUrlAsync(string url, CancellationToken ct)
    {
        using var httpClient = new HttpClient();
        var bytes = await httpClient.GetByteArrayAsync(url, ct);

        using var image = Image.Load(bytes);
        image.Mutate(img => img.Resize(new ResizeOptions
        {
            Size = new Size(800, 600),
            Mode = ResizeMode.Crop,
        }));

        using var outputStream = new MemoryStream();
        await image.SaveAsWebpAsync(outputStream, ct); 
        outputStream.Position = 0;

        var key = $"products/{Guid.NewGuid()}.webp";

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
        var prefix = $"{_endpoint}/{_bucketName}/";
        if (!path.StartsWith(prefix, StringComparison.Ordinal))
            throw new ArgumentException("The image does not belong to the configured storage.", nameof(path));

        var key = path[prefix.Length..];
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("The image path is invalid.", nameof(path));

        await s3Client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        }, ct);
    }
    public async Task<string> UpdateImageAsync(string oldPath, IFormFile newFile, ImageUploadKind kind, CancellationToken ct)
    {
        var newPath = await UploadImageAsync(newFile, kind, ct);
        await DeleteImageAsync(oldPath, ct);
        return newPath;
    }

    private static void ValidateFile(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("Select an image to upload.", nameof(file));
        if (file.Length > MaxImageSizeBytes)
            throw new ArgumentException("The image must not exceed 8 MB.", nameof(file));

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not ".png" and not ".jpg" and not ".jpeg" and not ".webp")
            throw new ArgumentException("Only PNG, JPG and WEBP images are supported.", nameof(file));
    }

    private static void ValidateDimensions(Image image)
    {
        if (image.Width < 16 || image.Height < 16 || image.Width > 12_000 || image.Height > 12_000)
            throw new ArgumentException("The image dimensions must be between 16 and 12000 pixels.");
    }

    private static (string Folder, Size Size) GetProfile(ImageUploadKind kind) => kind switch
    {
        ImageUploadKind.CompanyIcon => ("company-icons", new Size(256, 256)),
        ImageUploadKind.CompanyBanner => ("company-banners", new Size(1600, 500)),
        ImageUploadKind.Product => ("products", new Size(800, 600)),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
