using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Application.Services;

public class ImageService(IConfiguration configuration, ILogger<ImageService> logger) : IImageService
{
    public async Task<string> UploadImageAsync(IFormFile file, int maxWidth, int maxHeight, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            //Надалі, залежно від логіки контролера можна замінити на 
            //return BadRequest("Файл не вибрано.");
            throw new ArgumentException("File is null or empty.", nameof(file));
        }

        try
        {
            var fileName = $"{Path.GetRandomFileName()}.webp";
            var dirImageName = configuration["DirImageName"] ?? "images";
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), dirImageName);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = Path.Combine(uploadPath, fileName);

            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream, cancellationToken);

            // Це захищає від розтягування(upscaling) і втрати якості невеликих фото.
            if (image.Width > maxWidth || image.Height > maxHeight)
            {
                image.Mutate(imgc => imgc.Resize(new ResizeOptions
                {
                    Size = new Size(maxWidth, maxHeight),
                    Mode = ResizeMode.Max
                }));
            }

            await image.SaveAsync(filePath, new WebpEncoder(), cancellationToken);

            return fileName;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upload and process image.");
            throw;
        }
    }

    public bool DeleteImage(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;

        try
        {
            // Захист від Path Traversal: беремо ЛИШЕ ім'я файлу, ігноруючи будь-які ../
            var safeFileName = Path.GetFileName(fileName);

            var dirImageName = configuration["DirImageName"] ?? "images";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), dirImageName, safeFileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                logger.LogInformation("Image {FileName} was successfully deleted.", safeFileName);
            } 
            else
            {
                logger.LogWarning("Attempted to delete image {FileName}, but file was not found.", safeFileName);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting image {FileName}", fileName);
            return false;
        }
    }

    public string GetDefaultUserImage()
    {
        return configuration["DefaultUserImage"] ?? "default-user.webp";
    }
}
