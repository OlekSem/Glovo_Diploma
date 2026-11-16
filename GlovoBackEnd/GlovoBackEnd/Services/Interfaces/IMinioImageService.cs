namespace GlovoBackEnd.Services.Interfaces;


public interface IMinioImageService
{
    Task<string> UploadImageAsync(IFormFile file, CancellationToken ct);
    Task DeleteImageAsync(string path, CancellationToken ct);
    Task<string> UpdateImageAsync(string oldPath, IFormFile newFile, CancellationToken ct);
    Task<string> UploadImageFromUrlAsync(string url, CancellationToken ct);

}