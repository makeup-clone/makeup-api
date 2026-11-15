namespace Application.Models;

public class LoginModel
{
    /// <summary>
    /// Електронна пошта користувача
    /// </summary>
    /// <example>user1@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Пароль користувача
    /// </summary>
    /// <example>123456</example>
    public string Password { get; set; } = string.Empty;
}
