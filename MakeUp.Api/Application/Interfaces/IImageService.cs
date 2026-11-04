using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file, int maxWidth, int maxHeight, CancellationToken cancellationToken);
        bool DeleteImage(string fileNema);
        string GetDefaultUserImage();
    }
}
