using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Models;

public class RegisterModel
{
    [Required(ErrorMessage = "Ім'я є обов'язковим")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Прізвище є обов'язковим")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email є обов'язковим")]
    [EmailAddress(ErrorMessage = "Невірний формат email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль є обов'язковим")]
    [MinLength(6, ErrorMessage = "Пароль має містити щонайменше 6 символів")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердження пароля є обов'язковим")]
    [Compare("Password", ErrorMessage = "Паролі не збігаються")]
    public string PasswordConfirm { get; set; } = string.Empty;

    /// <summary>
    /// Зображення профілю
    /// </summary>
    public IFormFile? Image { get; set; }
}
