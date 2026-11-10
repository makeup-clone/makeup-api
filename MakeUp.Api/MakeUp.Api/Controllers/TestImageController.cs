using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MakeUp.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestImageController(IImageService imageService) : ControllerBase
    {
        /// <summary>
        /// Завантаження та ресайз зображення
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Файл не вибрано.");
            }

            try
            {
                int maxWidth = 1200;
                int maxHeight = 1200;

                var fileName = await imageService.UploadImageAsync(file, maxWidth, maxHeight, cancellationToken);

                return Ok(new
                {
                    Message = "Зображення успішно завантажено та збережено у форматі WebP.",
                    FileName = fileName
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, "Запит було скасовано клієнтом.");
            }
        }

        /// <summary>
        /// Видалення зображення за назвою файлу
        /// </summary>
        [HttpDelete]
        public IActionResult Delete(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest("Назву файлу не вказано.");
            }

            if (imageService.DeleteImage(fileName))
                return Ok(new { Message = $"Видалення файлу '{fileName}' успішно завершена." });
            else return BadRequest("Файл не знайдено або його вже було видалено.");
        }
    }
}
