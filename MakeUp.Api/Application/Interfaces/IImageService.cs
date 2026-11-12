using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IImageService
    {
        /// <summary>
        /// Асинхронно завантажує, оптимізує та змінює розмір зображення, зберігаючи його у форматі WebP.
        /// </summary>
        /// <param name="file">Файл зображення, отриманий з HTTP-запиту (IFormFile).</param>
        /// <param name="maxWidth">Максимальна ширина зображення в пікселях. Якщо оригінал менший, збільшення (upscaling) не виконується.</param>
        /// <param name="maxHeight">Максимальна висота зображення в пікселях.</param>
        /// <param name="cancellationToken">Токен скасування асинхронної операції.</param>
        /// <param name="quality">Якість стиснення у форматі WebP (від 1 до 100). За замовчуванням встановлено 80.</param>
        /// <returns>Повертає унікальне згенероване ім'я збереженого файлу з розширенням .webp.</returns>
        Task<string> UploadImageAsync(IFormFile file, int maxWidth, int maxHeight, CancellationToken cancellationToken = default, int quality = 80);
        /// <summary>
        /// Видаляє зображення за його унікальним ім'ям із локального сховища з урахуванням захисту від Path Traversal.
        /// </summary>
        /// <param name="fileName">Ім'я файлу зображення, яке потрібно видалити.</param>
        /// <returns>Повертає true, якщо файл успішно видалено; false, якщо файл не знайдено або сталася помилка.</returns>
        bool DeleteImage(string fileName);
        /// <summary>
        /// Повертає назву файла зображення користувача за замовчуванням із конфігурації.
        /// </summary>
        string GetDefaultUserImage();
    }
}
