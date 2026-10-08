using GenericStore.Domain.Entities;
using GenericStore.Domain.ValueObjects;

namespace GenericStore.Application.Abstractions;

public interface IStoreRepository
{
    Task<Store?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<Store>> GetByUserIdAsync(Guid userId, CancellationToken ct);
    Task<bool> SlugExistsAsync(Slug slug, CancellationToken ct);
    Task AddAsync(Store store, CancellationToken ct);

    Task UpdateAsync(Store store, CancellationToken ct);
}