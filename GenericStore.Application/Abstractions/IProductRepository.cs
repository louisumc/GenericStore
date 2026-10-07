using GenericStore.Domain.Entities;

namespace GenericStore.Application.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken ct);
    Task<IReadOnlyList<Product>> GetActiveByStoreIdAsync(Guid storeId, CancellationToken ct);
    Task AddAsync(Product product, CancellationToken ct);
    Task UpdateAsync(Product product, CancellationToken ct);
}