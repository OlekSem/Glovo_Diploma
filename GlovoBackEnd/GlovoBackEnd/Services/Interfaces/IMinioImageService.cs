namespace GlovoBackEnd.Services.Interfaces;

public enum ImageUploadKind
{
    CompanyIcon,
    CompanyBanner,
    Product
}

public interface IMinioImageService
{
    Task<string> UploadImageAsync(IFormFile file, ImageUploadKind kind, CancellationToken ct);
    Task DeleteImageAsync(string path, CancellationToken ct);
    Task<string> UpdateImageAsync(string oldPath, IFormFile newFile, ImageUploadKind kind, CancellationToken ct);
    Task<string> UploadImageFromUrlAsync(string url, CancellationToken ct);

}
