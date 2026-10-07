using GenericStore.Application.Abstractions;
using GenericStore.Application.DTOs.Users;
using GenericStore.Domain.Entities;
using GenericStore.Domain.Exceptions;
using GenericStore.Domain.ValueObjects;

namespace GenericStore.Application.UseCases;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository) => _repository = repository;

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct)
    {
        var email = new Email(request.Email);

        if (await _repository.EmailExistsAsync(email, ct))
            throw new ConflictException("E-mail já cadastrado.");

        var user = new User(request.Name, email);

        await _repository.AddAsync(user, ct);

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