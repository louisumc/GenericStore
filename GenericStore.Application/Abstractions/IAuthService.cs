using GenericStore.Application.DTOs.Auth;

namespace GenericStore.Application.Abstractions;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct);
}