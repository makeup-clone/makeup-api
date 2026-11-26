using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface ICookieService
{
    void SetTokenCookie(HttpResponse response, string token, string refreshToken, bool rememberMe = false);
    void DeleteTokenCookie(HttpResponse response);
}
