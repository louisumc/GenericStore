using GenericStore.Domain.Entities;

namespace GenericStore.Application.Abstractions;

public interface ITokenService
{
    (string token, DateTime expiresAt) GenerateToken(User user);
}