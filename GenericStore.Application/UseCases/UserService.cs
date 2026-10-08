using GenericStore.Application.Abstractions;
using GenericStore.Application.DTOs.Users;
using GenericStore.Domain.Entities;
using GenericStore.Domain.Exceptions;
using GenericStore.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GenericStore.Application.UseCases;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository repository,
        IPasswordHasher passwordHasher,
        ILogger<UserService> logger)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new DomainException("Senha deve ter no mínimo 6 caracteres.");

        var email = new Email(request.Email);

        if (await _repository.EmailExistsAsync(email, ct))
        {
            _logger.LogWarning("Tentativa de criar usuário com e-mail duplicado: {Email}", email.Value);
            throw new ConflictException("E-mail já cadastrado.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new User(request.Name, email, passwordHash);

        await _repository.AddAsync(user, ct);

        _logger.LogInformation("Usuário criado: {UserId} ({Email})", user.Id, user.Email.Value);

        return ToResponse(user);
    }

    public async Task<UserResponse> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var user = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Usuário não encontrado.");

        return ToResponse(user);
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken ct)
    {
        var users = await _repository.GetAllAsync(ct);
        return users.Select(ToResponse).ToList();
    }

    private static UserResponse ToResponse(User user) =>
        new(user.Id, user.Name, user.Email.Value, user.CreatedAt);
}