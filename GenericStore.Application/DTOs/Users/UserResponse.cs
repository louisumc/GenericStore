namespace GenericStore.Application.DTOs.Users;

public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    DateTime CreatedAt);