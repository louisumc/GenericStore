using GenericStore.Application.Abstractions;
using GenericStore.Application.DTOs.Products;
using GenericStore.Domain.Entities;
using GenericStore.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace GenericStore.Application.UseCases;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        IStoreRepository storeRepository,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _storeRepository = storeRepository;
        _logger = logger;
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        var store = await _storeRepository.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Loja não encontrada.");

        if (!store.Active)
            throw new DomainException("Loja inativa não pode receber novos produtos.");

        var product = new Product(
            store.Id,
            request.Name,
            request.Description,
            request.Price,
            request.Stock);

        await _productRepository.AddAsync(product, ct);

        _logger.LogInformation("Produto criado: {ProductId} ({Name}) na loja {StoreId}",
            product.Id, product.Name, product.StoreId);

        return ToResponse(product);
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Produto não encontrado.");

        return ToResponse(product);
    }

    public async Task<IReadOnlyList<ProductResponse>> GetAllActiveAsync(CancellationToken ct)
    {
        var products = await _productRepository.GetActiveAsync(ct);
        return products.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<ProductResponse>> GetActiveByStoreIdAsync(Guid storeId, CancellationToken ct)
    {
        _ = await _storeRepository.GetByIdAsync(storeId, ct)
            ?? throw new NotFoundException("Loja não encontrada.");

        var products = await _productRepository.GetActiveByStoreIdAsync(storeId, ct);
        return products.Select(ToResponse).ToList();
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Produto não encontrado.");

        product.Update(request.Name, request.Description, request.Price, request.Stock);

        await _productRepository.UpdateAsync(product, ct);

        _logger.LogInformation("Produto atualizado: {ProductId} ({Name})", product.Id, product.Name);

        return ToResponse(product);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Produto não encontrado.");

        product.Deactivate();
        await _productRepository.UpdateAsync(product, ct);

        _logger.LogInformation("Produto desativado: {ProductId}", product.Id);
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Produto não encontrado.");

        product.Activate();
        await _productRepository.UpdateAsync(product, ct);

        _logger.LogInformation("Produto ativado: {ProductId}", product.Id);
    }

    private static ProductResponse ToResponse(Product product) =>
        new(product.Id,
            product.StoreId,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.Active,
            product.CreatedAt,
            product.UpdatedAt);
}