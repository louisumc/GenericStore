using GenericStore.Domain.Entities;
using GenericStore.Domain.ValueObjects;

namespace GenericStore.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct);
    Task<bool> EmailExistsAsync(Email email, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
}