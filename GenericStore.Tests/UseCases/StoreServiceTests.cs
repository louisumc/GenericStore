using FluentAssertions;
using GenericStore.Application.Abstractions;
using GenericStore.Application.DTOs.Stores;
using GenericStore.Application.UseCases;
using GenericStore.Domain.Entities;
using GenericStore.Domain.Exceptions;
using GenericStore.Domain.ValueObjects;
using NSubstitute;

namespace GenericStore.Tests.UseCases;

public class StoreServiceTests
{
    private readonly IStoreRepository _storeRepository = Substitute.For<IStoreRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly StoreService _sut;

    public StoreServiceTests()
    {
        _sut = new StoreService(_storeRepository, _userRepository);
    }

    private User CriarUsuarioExistente()
        => new("João Silva", new Email("joao@email.com"));

    [Fact]
    public async Task CreateAsync_ComDadosValidos_DeveRetornarLojaAtiva()
    {
        // Arrange
        var user = CriarUsuarioExistente();
        var request = new CreateStoreRequest(user.Id, "Minha Loja", "minha-loja");

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _storeRepository.SlugExistsAsync(Arg.Any<Slug>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Minha Loja");
        result.Slug.Should().Be("minha-loja");
        result.Active.Should().BeTrue();
        result.UserId.Should().Be(user.Id);

        await _storeRepository.Received(1).AddAsync(Arg.Any<Store>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ComUsuarioInexistente_DeveLancarNotFoundException()
    {
        // Arrange
        var request = new CreateStoreRequest(Guid.NewGuid(), "Minha Loja", "minha-loja");
        _userRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Usuário*");
    }

    [Fact]
    public async Task CreateAsync_ComNomeInvalido_DeveLancarDomainException()
    {
        // Arrange
        var user = CriarUsuarioExistente();
        var request = new CreateStoreRequest(user.Id, "Lo", "loja-valida");

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*entre 3 e 100*");
    }

    [Fact]
    public async Task CreateAsync_ComSlugInvalido_DeveLancarDomainException()
    {
        // Arrange
        var user = CriarUsuarioExistente();
        var request = new CreateStoreRequest(user.Id, "Minha Loja", "Slug Inválido!");

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Slug inválido*");
    }

    [Fact]
    public async Task CreateAsync_ComSlugDuplicado_DeveLancarConflictException()
    {
        // Arrange
        var user = CriarUsuarioExistente();
        var request = new CreateStoreRequest(user.Id, "Minha Loja", "minha-loja");

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _storeRepository.SlugExistsAsync(Arg.Any<Slug>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Slug*");
    }

    [Fact]
    public async Task GetByIdAsync_ComIdInexistente_DeveLancarNotFoundException()
    {
        // Arrange
        _storeRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Store?)null);

        // Act
        var act = async () => await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByUserIdAsync_ComUsuarioInexistente_DeveLancarNotFoundException()
    {
        // Arrange
        _userRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = async () => await _sut.GetByUserIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}