using Application.Models;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponceModel> RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default);
}
