using Application.Interfaces;
using Application.Models;
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
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        try
        {
            var authResponse = await authService.LoginAsync(model);
            cookieService.SetTokenCookie(Response, authResponse.AccessToken, authResponse.RefreshToken);
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
        cookieService.DeleteTokenCookie(Response);
        return Ok(new { message = "Успішний вихід з системи" });
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

        var roles = User.FindAll("roles")
            .Select(c => c.Value)
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
            providers,
            roles
        });
    }

    [HttpPost]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginModel model)
    {
        try
        {
            var authResponse = await authService.GoogleLoginAsync(model);
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
