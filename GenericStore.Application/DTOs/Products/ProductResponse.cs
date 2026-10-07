namespace GenericStore.Application.DTOs.Products;

public record ProductResponse(
    Guid Id,
    Guid StoreId,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    bool Active,
    DateTime CreatedAt,
    DateTime? UpdatedAt);