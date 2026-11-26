using Application.Interfaces;
using Application.Models;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MakeUp.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController(IAuthService authService, ICookieService cookieService) : ControllerBase
{
    
    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterModel model, CancellationToken cancellationToken)
    {
        try
        {
            var authResponse = await authService.RegisterAsync(model, cancellationToken);
            cookieService.SetTokenCookie(Response, authResponse.AccessToken, authResponse.RefreshToken);
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
            var authResponse = await authService.LoginAsync(model, cancellationToken);
            cookieService.SetTokenCookie(Response, authResponse.AccessToken, authResponse.RefreshToken);
            return Ok(new { message = "Успішний вхід" });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[CookieConstants.RefreshTokenCookieName];
        await authService.LogoutAsync(refreshToken, cancellationToken);

        cookieService.DeleteTokenCookie(Response);
        return Ok(new { message = "Успішний вихід із системи" });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var providers = User.FindAll("providers")
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct()
            .ToArray();

        return Ok(new
        {
            id = User.FindFirstValue("id"),
            email = User.FindFirstValue("email"),
            firstName = User.FindFirstValue("firstName"),
            lastName = User.FindFirstValue("lastName"),
            image = User.FindFirstValue("image"),
            createdAt = User.FindFirstValue("createdAt"),
            roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value),
            providers,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[CookieConstants.RefreshTokenCookieName];

        try
        {
            var authResponse = await authService.RefreshTokenAsync(refreshToken, cancellationToken);
            cookieService.SetTokenCookie(Response, authResponse.AccessToken, authResponse.RefreshToken);
            return Ok(new { message = "Токен успішно оновлено" });
        }
        catch (UnauthorizedAccessException ex)
        {
            cookieService.DeleteTokenCookie(Response);
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginModel model, CancellationToken cancellationToken)
    {
        try
        {
            var authResponse = await authService.GoogleLoginAsync(model, cancellationToken);
            cookieService.SetTokenCookie(Response, authResponse.AccessToken, authResponse.RefreshToken);
            return Ok(new { message = "Успішний вхід через Google" });
        }
        catch  (Exception ex) 
        {
            return Unauthorized(new {message = ex.Message}); 
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> AuthTest()
    {
        return Ok(new { message = "Користувач авторизований" });
    }
}
