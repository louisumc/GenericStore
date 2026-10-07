using GenericStore.Application.DTOs.Users;

namespace GenericStore.Application.Abstractions;

public interface IUserService
{
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct);
    Task<UserResponse> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken ct);
}