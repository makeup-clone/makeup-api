using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace Application.Services;

public class ImageService(IConfiguration configuration, ILogger<ImageService> logger) : IImageService
{
    public async Task<string> UploadImageAsync(IFormFile file, int maxWidth, int maxHeight, CancellationToken cancellationToken = default, int quality = 80)
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
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = file.OpenReadStream();
            using var originalBitmap = SKBitmap.Decode(stream);

            if (originalBitmap == null)
                throw new InvalidOperationException("Failed to decode the image file.");

            SKBitmap bitmapToSave = originalBitmap;
            bool needsResize = originalBitmap.Width > maxWidth || originalBitmap.Height > maxHeight;

            if (needsResize)
            {
                double widthRatio = (double)maxWidth / originalBitmap.Width;
                double heightRatio = (double)maxHeight / originalBitmap.Height;
                double ratio = Math.Min(widthRatio, heightRatio);

                int newWidth = (int)(originalBitmap.Width * ratio);
                int newHeight = (int)(originalBitmap.Height * ratio);

                var resizedBitmap = originalBitmap.Resize(new SKImageInfo(newWidth, newHeight), SKSamplingOptions.Default);

                if (resizedBitmap != null)
                    bitmapToSave = resizedBitmap;
            }

            try
            {
                using var image = SKImage.FromBitmap(bitmapToSave);
                using var encodedData = image.Encode(SKEncodedImageFormat.Webp, quality);

                await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
                await using var dataStream = encodedData.AsStream();

                await dataStream.CopyToAsync(fileStream, cancellationToken);
            }
            finally
            {
                // Якщо створювався окремий бітмап для ресайзу — звільняємо пам'ять
                if (bitmapToSave != originalBitmap)
                    bitmapToSave.Dispose();
            }

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
