using Application.Interfaces;
using Domain.Constants;
using Microsoft.AspNetCore.Http;

namespace Application.Services;

public class CookieService : ICookieService
{
    public void SetTokenCookie(HttpResponse response, string accesstoken, string refreshToken)
    {
        var accessOptions = new CookieOptions
        {
            HttpOnly = true, // JavaScript у браузері НЕ має доступу до цієї куки (захист від XSS)
            Secure = false, // Передається тільки через HTTPS (false для локальної розробки без HTTPS, якщо потрібно)
            SameSite = SameSiteMode.Strict, // Захист від CSRF-атак (або Lax, якщо фронтенд і бекенд на різних субдоменах)
            Expires = DateTimeOffset.UtcNow.AddMinutes(CookieConstants.AccessTokenExpirationMinutes) // Термін життя куки
        };

        var refreshOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Path = "/api/Auth",
            Expires = DateTimeOffset.UtcNow.AddDays(CookieConstants.RefreshTokenExpirationDays)
        };

        response.Cookies.Append(CookieConstants.AccessTokenCookieName, accesstoken, accessOptions);

        response.Cookies.Append(CookieConstants.RefreshTokenCookieName, refreshToken, refreshOptions);
    }

    public void DeleteTokenCookie(HttpResponse response)
    {
        var accessOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict
        };

        var refreshOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Path = "/api/Auth" // має збігатися з Path при створенні!
        };

        response.Cookies.Delete(CookieConstants.AccessTokenCookieName, accessOptions);
        response.Cookies.Delete(CookieConstants.RefreshTokenCookieName, refreshOptions);
    }
}
