namespace Domain.Constants;

public static class CookieConstants
{
    public const string AccessTokenCookieName = "access_token";
    public const int AccessTokenExpirationMinutes = 15;
    public const string RefreshTokenCookieName = "refresh_token";
    public const int RefreshTokenExpirationDays = 30;
}
