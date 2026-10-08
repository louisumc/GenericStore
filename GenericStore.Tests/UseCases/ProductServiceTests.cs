using FluentAssertions;
using GenericStore.Application.Abstractions;
using GenericStore.Application.DTOs.Products;
using GenericStore.Application.UseCases;
using GenericStore.Domain.Entities;
using GenericStore.Domain.Exceptions;
using GenericStore.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace GenericStore.Tests.UseCases;

public class ProductServiceTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IStoreRepository _storeRepository = Substitute.For<IStoreRepository>();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_productRepository, _storeRepository, NullLogger<ProductService>.Instance);
    }

    private Store CriarLojaAtiva()
    {
        var store = new Store(Guid.NewGuid(), "Minha Loja", new Slug("minha-loja"));
        return store;
    }

    private Store CriarLojaInativa()
    {
        var store = CriarLojaAtiva();
        store.Deactivate();
        return store;
    }

    private Product CriarProduto(Guid storeId)
        => new(storeId, "Notebook Dell", "Notebook para trabalho", 4500m, 10);

    [Fact]
    public async Task CreateAsync_ComDadosValidos_DeveRetornarProdutoAtivo()
    {
        // Arrange
        var store = CriarLojaAtiva();
        var request = new CreateProductRequest(store.Id, "Notebook Dell", "Notebook", 4500m, 10);

        _storeRepository.GetByIdAsync(store.Id, Arg.Any<CancellationToken>())
            .Returns(store);

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Notebook Dell");
        result.Price.Should().Be(4500m);
        result.Active.Should().BeTrue();

        await _productRepository.Received(1).AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ComLojaInexistente_DeveLancarNotFoundException()
    {
        // Arrange
        var request = new CreateProductRequest(Guid.NewGuid(), "Notebook", null, 100m, 1);
        _storeRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Store?)null);

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Loja*");
    }

    [Fact]
    public async Task CreateAsync_ComLojaInativa_DeveLancarDomainException()
    {
        // Arrange
        var store = CriarLojaInativa();
        var request = new CreateProductRequest(store.Id, "Notebook", null, 100m, 1);

        _storeRepository.GetByIdAsync(store.Id, Arg.Any<CancellationToken>())
            .Returns(store);

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*inativa*");
    }

    [Fact]
    public async Task CreateAsync_ComNomeInvalido_DeveLancarDomainException()
    {
        // Arrange
        var store = CriarLojaAtiva();
        var request = new CreateProductRequest(store.Id, "Ab", null, 100m, 1);

        _storeRepository.GetByIdAsync(store.Id, Arg.Any<CancellationToken>())
            .Returns(store);

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*entre 3 e 150*");
    }

    [Fact]
    public async Task CreateAsync_ComPrecoInvalido_DeveLancarDomainException()
    {
        // Arrange
        var store = CriarLojaAtiva();
        var request = new CreateProductRequest(store.Id, "Produto Válido", null, 0m, 1);

        _storeRepository.GetByIdAsync(store.Id, Arg.Any<CancellationToken>())
            .Returns(store);

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Preço*");
    }

    [Fact]
    public async Task CreateAsync_ComEstoqueInvalido_DeveLancarDomainException()
    {
        // Arrange
        var store = CriarLojaAtiva();
        var request = new CreateProductRequest(store.Id, "Produto Válido", null, 100m, -5);

        _storeRepository.GetByIdAsync(store.Id, Arg.Any<CancellationToken>())
            .Returns(store);

        // Act
        var act = async () => await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Estoque*");
    }

    [Fact]
    public async Task UpdateAsync_ComDadosValidos_DeveAtualizarEAtualizarUpdatedAt()
    {
        // Arrange
        var store = CriarLojaAtiva();
        var product = CriarProduto(store.Id);
        var request = new UpdateProductRequest("Notebook Dell Inspiron", "Atualizado", 4299.90m, 15);

        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        var result = await _sut.UpdateAsync(product.Id, request, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Notebook Dell Inspiron");
        result.Price.Should().Be(4299.90m);
        result.Stock.Should().Be(15);
        result.UpdatedAt.Should().NotBeNull();

        await _productRepository.Received(1).UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ComProdutoInexistente_DeveLancarNotFoundException()
    {
        // Arrange
        var request = new UpdateProductRequest("Nome", null, 100m, 1);
        _productRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        // Act
        var act = async () => await _sut.UpdateAsync(Guid.NewGuid(), request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeactivateAsync_DeveMarcarProdutoComoInativo()
    {
        // Arrange
        var store = CriarLojaAtiva();
        var product = CriarProduto(store.Id);

        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        await _sut.DeactivateAsync(product.Id, CancellationToken.None);

        // Assert
        product.Active.Should().BeFalse();
        await _productRepository.Received(1).UpdateAsync(product, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivateAsync_DeveMarcarProdutoComoAtivo()
    {
        // Arrange
        var store = CriarLojaAtiva();
        var product = CriarProduto(store.Id);
        product.Deactivate();

        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        await _sut.ActivateAsync(product.Id, CancellationToken.None);

        // Assert
        product.Active.Should().BeTrue();
        await _productRepository.Received(1).UpdateAsync(product, Arg.Any<CancellationToken>());
    }
}