namespace GenericStore.Application.DTOs.Products;

public record CreateProductRequest(
    Guid StoreId,
    string Name,
    string? Description,
    decimal Price,
    int Stock);