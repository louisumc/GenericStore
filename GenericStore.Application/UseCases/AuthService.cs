using GenericStore.Application.Abstractions;
using GenericStore.Application.DTOs.Auth;
using GenericStore.Domain.Exceptions;
using GenericStore.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GenericStore.Application.UseCases;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        Email email;
        try
        {
            email = new Email(request.Email);
        }
        catch (DomainException)
        {
            throw new DomainException("Credenciais inválidas.");
        }

        var user = await _userRepository.GetByEmailAsync(email, ct);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Tentativa de login inválida para {Email}", request.Email);
            throw new DomainException("Credenciais inválidas.");
        }

        var (token, expiresAt) = _tokenService.GenerateToken(user);

        _logger.LogInformation("Login bem-sucedido para {Email}", user.Email.Value);

        return new LoginResponse(token, expiresAt, user.Id, user.Name, user.Email.Value);
    }
}