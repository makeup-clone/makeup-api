using Application.Models;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseModel> RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default);

    Task<AuthResponseModel> LoginAsync(LoginModel model, CancellationToken cancellationToken = default);

    Task<AuthResponseModel> GoogleLoginAsync(GoogleLoginModel model, CancellationToken cancellationToken = default);
}
