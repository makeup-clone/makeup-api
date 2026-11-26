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

    /// <summary>
    /// Запам'ятати мене (false - коротка сесія)
    /// </summary>
    /// <example>false</example>
    public bool RememberMe { get; set; } = false;
}