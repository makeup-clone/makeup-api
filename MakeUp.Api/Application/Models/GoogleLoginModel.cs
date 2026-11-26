namespace Application.Models;

public class GoogleLoginModel
{
    public required string GoogleIdToken { get; set; }

    /// <summary>
    /// Запам'ятати мене (false - коротка сесія)
    /// </summary>
    /// <example>false</example>
    public bool RememberMe { get; set; } = false;
}
