using GenericStore.Application.Abstractions;
using GenericStore.Application.DTOs.Stores;
using GenericStore.Domain.Entities;
using GenericStore.Domain.Exceptions;
using GenericStore.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GenericStore.Application.UseCases;

public class StoreService : IStoreService
{
    private readonly IStoreRepository _storeRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<StoreService> _logger;

    public StoreService(
        IStoreRepository storeRepository,
        IUserRepository userRepository,
        ILogger<StoreService> logger)
    {
        _storeRepository = storeRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<StoreResponse> CreateAsync(CreateStoreRequest request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException("Usuário não encontrado.");

        var slug = new Slug(request.Slug);

        if (await _storeRepository.SlugExistsAsync(slug, ct))
        {
            _logger.LogWarning("Tentativa de criar loja com slug duplicado: {Slug}", slug.Value);
            throw new ConflictException("Slug já cadastrado.");
        }

        var store = new Store(user.Id, request.Name, slug);

        await _storeRepository.AddAsync(store, ct);

        _logger.LogInformation("Loja criada: {StoreId} ({Slug}) para usuário {UserId}",
            store.Id, store.Slug.Value, store.UserId);

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

    public async Task ActivateAsync(Guid id, CancellationToken ct)
    {
        var store = await _storeRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Loja não encontrada.");

        store.Activate();
        await _storeRepository.UpdateAsync(store, ct);

        _logger.LogInformation("Loja ativada: {StoreId}", store.Id);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct)
    {
        var store = await _storeRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Loja não encontrada.");

        store.Deactivate();
        await _storeRepository.UpdateAsync(store, ct);

        _logger.LogInformation("Loja desativada: {StoreId}", store.Id);
    }

    private static StoreResponse ToResponse(Store store) =>
        new(store.Id, store.UserId, store.Name, store.Slug.Value, store.Active, store.CreatedAt);
}