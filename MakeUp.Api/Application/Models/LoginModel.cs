namespace Application.Models;

public class LoginModel
{
    /// <summary>
    /// Електронна пошта користувача
    /// </summary>
    /// <example>user@gmail.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Пароль користувача
    /// </summary>
    /// <example>User123</example>
    public string Password { get; set; } = string.Empty;
}
