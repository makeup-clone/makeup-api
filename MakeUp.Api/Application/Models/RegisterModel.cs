using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Models;

public class RegisterModel
{
    /// <summary>
    /// Ім'я
    /// </summary>
    /// <example>exampleFirtsName</example>
    [Required(ErrorMessage = "Ім'я є обов'язковим")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Прізвище
    /// </summary>
    /// <example>exampleLastName</example>
    [Required(ErrorMessage = "Прізвище є обов'язковим")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Електронна адреса
    /// </summary>
    /// <example>exampleEmail@gmail.com</example>
    [Required(ErrorMessage = "Email є обов'язковим")]
    [EmailAddress(ErrorMessage = "Невірний формат email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Пароль
    /// </summary>
    /// <example>examplePass123</example>
    [Required(ErrorMessage = "Пароль є обов'язковим")]
    [MinLength(6, ErrorMessage = "Пароль має містити щонайменше 6 символів")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Підтвердження пароля
    /// </summary>
    /// <example>examplePass123</example>
    [Required(ErrorMessage = "Підтвердження пароля є обов'язковим")]
    [Compare("Password", ErrorMessage = "Паролі не збігаються")]
    public string PasswordConfirm { get; set; } = string.Empty;

    /// <summary>
    /// Зображення профілю
    /// </summary>
    public IFormFile? Image { get; set; }

    /// <summary>
    /// Запам'ятати мене (false - коротка сесія)
    /// </summary>
    /// <example>false</example>
    public bool RememberMe { get; set; } = false;
}
