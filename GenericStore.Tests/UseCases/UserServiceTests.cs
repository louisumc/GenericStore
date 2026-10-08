using FluentAssertions;
using GenericStore.Application.Abstractions;
using GenericStore.Application.DTOs.Users;
using GenericStore.Application.UseCases;
using GenericStore.Domain.Entities;
using GenericStore.Domain.Exceptions;
using GenericStore.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace GenericStore.Tests.UseCases;

public class UserServiceTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_repository, NullLogger<UserService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ComDadosValidos_DeveRetornarUsuario()
    {
        // Arrange
        var request = new CreateUserRequest("João Silva", "joao@email.com");
        _repository.EmailExistsAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("João Silva");
        result.Email.Should().Be("joao@email.com");
        await _repository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ComNomeInvalido_DeveLancarDomainException()
    {
        // Arrange
        var request = new CreateUserRequest("Jo", "joao@email.com");

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*entre 3 e 100*");
    }

    [Fact]
    public async Task CreateAsync_ComEmailInvalido_DeveLancarDomainException()
    {
        // Arrange
        var request = new CreateUserRequest("João Silva", "nao-eh-email");

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*E-mail inválido*");
    }

    [Fact]
    public async Task CreateAsync_ComEmailDuplicado_DeveLancarConflictException()
    {
        // Arrange
        var request = new CreateUserRequest("João Silva", "joao@email.com");
        _repository.EmailExistsAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*já cadastrado*");
    }

    [Fact]
    public async Task GetByIdAsync_ComIdInexistente_DeveLancarNotFoundException()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = async () => await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}