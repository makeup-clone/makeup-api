using Application.Interfaces;
using Application.Models;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MakeUp.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private void SetTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // JavaScript у браузері НЕ має доступу до цієї куки (захист від XSS)
            Secure = false, // Передається тільки через HTTPS (false для локальної розробки без HTTPS, якщо потрібно)
            SameSite = SameSiteMode.Strict, // Захист від CSRF-атак (або Lax, якщо фронтенд і бекенд на різних субдоменах)
            Expires = DateTimeOffset.UtcNow.AddDays(7) // Термін життя куки
        };

        Response.Cookies.Append(CookieConstants.AccessTokenCookieName, token, cookieOptions);
    }
    private void DeleteTokenCookie()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Має збігатися з параметром при встановленні
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        };

        Response.Cookies.Delete(CookieConstants.AccessTokenCookieName, cookieOptions);
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterModel model, CancellationToken cancellationToken)
    {
        try
        {
            var authResponce = await authService.RegisterAsync(model, cancellationToken);
            SetTokenCookie(authResponce.AccessToken);
            return Ok(new { message = "Реєстрація успішна" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message});
        }
        
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginModel model, CancellationToken cancellationToken)
    {
        try
        {
            var authResponce = await authService.LoginAsync(model, cancellationToken);
            SetTokenCookie(authResponce.AccessToken);
            return Ok(new { message = "Успішний вхід" });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        DeleteTokenCookie();
        return Ok(new { message = "Успішний вихід з системи" });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> AuthTest()
    {
        return Ok(new { message = "Користувач авторизований" });
    }
}
