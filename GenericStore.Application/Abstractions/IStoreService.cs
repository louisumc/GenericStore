using GenericStore.Application.DTOs.Stores;

namespace GenericStore.Application.Abstractions;

public interface IStoreService
{
    Task<StoreResponse> CreateAsync(CreateStoreRequest request, CancellationToken ct);
    Task<StoreResponse> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<StoreResponse>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<StoreResponse>> GetByUserIdAsync(Guid userId, CancellationToken ct);

    Task ActivateAsync(Guid id, CancellationToken ct);
    Task DeactivateAsync(Guid id, CancellationToken ct);
}