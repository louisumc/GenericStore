using GenericStore.Application.Abstractions;
using GenericStore.Application.DTOs.Stores;
using GenericStore.Domain.Entities;
using GenericStore.Domain.Exceptions;
using GenericStore.Domain.ValueObjects;

namespace GenericStore.Application.UseCases;

public class StoreService : IStoreService
{
    private readonly IStoreRepository _storeRepository;
    private readonly IUserRepository _userRepository;

    public StoreService(IStoreRepository storeRepository, IUserRepository userRepository)
    {
        _storeRepository = storeRepository;
        _userRepository = userRepository;
    }

    public async Task<StoreResponse> CreateAsync(CreateStoreRequest request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException("Usuário não encontrado.");

        var slug = new Slug(request.Slug);

        if (await _storeRepository.SlugExistsAsync(slug, ct))
            throw new ConflictException("Slug já cadastrado.");

        var store = new Store(user.Id, request.Name, slug);

        await _storeRepository.AddAsync(store, ct);

        return ToResponse(store);
    }

    public async Task<StoreResponse> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var store = await _storeRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Loja não encontrada.");

        return ToResponse(store);
    }

    public async Task<IReadOnlyList<StoreResponse>> GetAllAsync(CancellationToken ct)
    {
        var stores = await _storeRepository.GetAllAsync(ct);
        return stores.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<StoreResponse>> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        _ = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("Usuário não encontrado.");

        var stores = await _storeRepository.GetByUserIdAsync(userId, ct);
        return stores.Select(ToResponse).ToList();
    }

    private static StoreResponse ToResponse(Store store) =>
        new(store.Id, store.UserId, store.Name, store.Slug.Value, store.Active, store.CreatedAt);
}