namespace GenericStore.Application.DTOs.Users;

public record CreateUserRequest(string Name, string Email, string Password);