using dotnet_test.Controllers.Contracts;

namespace dotnet_test.Services.Auth;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
