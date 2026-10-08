namespace GenericStore.Application.DTOs.Auth;

public record LoginResponse(string Token, DateTime ExpiresAt, Guid UserId, string Name, string Email);