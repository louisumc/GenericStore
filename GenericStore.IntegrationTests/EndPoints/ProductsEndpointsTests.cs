using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GenericStore.Application.DTOs.Products;
using GenericStore.Application.DTOs.Stores;

namespace GenericStore.IntegrationTests.Endpoints;

public class ProductsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ProductsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static string SlugUnico() => $"loja-{Guid.NewGuid():N}";

    private async Task<(HttpClient client, Guid storeId)> SetupAsync()
    {
        var client = _factory.CreateClient();
        var login = await client.RegistrarELogarAsync();

        var storePayload = new { userId = login.UserId, name = "Loja", slug = SlugUnico() };
        var storeResp = await client.PostAsJsonAsync("/api/stores", storePayload);
        var store = await storeResp.Content.ReadFromJsonAsync<StoreResponse>();

        return (client, store!.Id);
    }

    [Fact]
    public async Task POST_ComDadosValidos_DeveRetornar201()
    {
        var (client, storeId) = await SetupAsync();
        var payload = new
        {
            storeId,
            name = "Notebook Dell",
            description = "Notebook para trabalho",
            price = 4500.00m,
            stock = 10
        };

        var response = await client.PostAsJsonAsync("/api/products", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ProductResponse>();
        body!.Active.Should().BeTrue();
    }

    [Fact]
    public async Task POST_ComPrecoInvalido_DeveRetornar400()
    {
        var (client, storeId) = await SetupAsync();
        var payload = new
        {
            storeId,
            name = "Produto Válido",
            description = (string?)null,
            price = 0m,
            stock = 1
        };

        var response = await client.PostAsJsonAsync("/api/products", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ComLojaInexistente_DeveRetornar404()
    {
        var client = _factory.CreateClient();
        await client.RegistrarELogarAsync();

        var payload = new
        {
            storeId = Guid.NewGuid(),
            name = "Produto Válido",
            description = (string?)null,
            price = 100m,
            stock = 1
        };

        var response = await client.PostAsJsonAsync("/api/products", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_DeveDesativarProduto_ERetornar204()
    {
        var (client, storeId) = await SetupAsync();
        var createPayload = new
        {
            storeId,
            name = "Notebook Dell",
            description = (string?)null,
            price = 4500m,
            stock = 10
        };
        var createResp = await client.PostAsJsonAsync("/api/products", createPayload);
        var produto = await createResp.Content.ReadFromJsonAsync<ProductResponse>();

        var response = await client.DeleteAsync($"/api/products/{produto!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listaResp = await client.GetAsync("/api/products");
        var lista = await listaResp.Content.ReadFromJsonAsync<List<ProductResponse>>();
        lista.Should().NotContain(p => p.Id == produto.Id);
    }

    [Fact]
    public async Task PATCH_Activate_DeveReativarProduto()
    {
        var (client, storeId) = await SetupAsync();
        var createPayload = new
        {
            storeId,
            name = "Notebook Dell",
            description = (string?)null,
            price = 4500m,
            stock = 10
        };
        var createResp = await client.PostAsJsonAsync("/api/products", createPayload);
        var produto = await createResp.Content.ReadFromJsonAsync<ProductResponse>();
        await client.DeleteAsync($"/api/products/{produto!.Id}");

        var response = await client.PatchAsync($"/api/products/{produto.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResp = await client.GetAsync($"/api/products/{produto.Id}");
        var atualizado = await getResp.Content.ReadFromJsonAsync<ProductResponse>();
        atualizado!.Active.Should().BeTrue();
    }

    [Fact]
    public async Task PUT_ComDadosValidos_DeveRetornar200_EAtualizarUpdatedAt()
    {
        var (client, storeId) = await SetupAsync();
        var createPayload = new
        {
            storeId,
            name = "Notebook Dell",
            description = "Original",
            price = 4500m,
            stock = 10
        };
        var createResp = await client.PostAsJsonAsync("/api/products", createPayload);
        var produto = await createResp.Content.ReadFromJsonAsync<ProductResponse>();

        var updatePayload = new
        {
            name = "Notebook Dell Inspiron",
            description = "Atualizado",
            price = 4299.90m,
            stock = 15
        };

        var response = await client.PutAsJsonAsync($"/api/products/{produto!.Id}", updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ProductResponse>();
        body!.Name.Should().Be("Notebook Dell Inspiron");
        body.UpdatedAt.Should().NotBeNull();
    }
}