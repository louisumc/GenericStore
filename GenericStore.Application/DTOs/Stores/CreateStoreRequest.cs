namespace GenericStore.Application.DTOs.Stores;

public record CreateStoreRequest(Guid UserId, string Name, string Slug);