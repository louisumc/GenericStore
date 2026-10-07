namespace GenericStore.Application.DTOs.Stores;

public record StoreResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string Slug,
    bool Active,
    DateTime CreatedAt);