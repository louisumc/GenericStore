using GenericStore.Application.DTOs.Products;

namespace GenericStore.Application.Abstractions;

public interface IProductService
{
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct);
    Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ProductResponse>> GetAllActiveAsync(CancellationToken ct);
    Task<IReadOnlyList<ProductResponse>> GetActiveByStoreIdAsync(Guid storeId, CancellationToken ct);
    Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct);
    Task DeactivateAsync(Guid id, CancellationToken ct);
    Task ActivateAsync(Guid id, CancellationToken ct);
}