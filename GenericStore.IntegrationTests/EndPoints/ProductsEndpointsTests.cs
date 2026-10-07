using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GenericStore.Application.DTOs.Products;
using GenericStore.Application.DTOs.Stores;
using GenericStore.Application.DTOs.Users;

namespace GenericStore.IntegrationTests.Endpoints;

public class ProductsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static string EmailUnico() => $"{Guid.NewGuid():N}@email.com";
    private static string SlugUnico() => $"loja-{Guid.NewGuid():N}";

    private async Task<Guid> CriarLojaAtiva()
    {
        var userPayload = new { name = "Owner", email = EmailUnico() };
        var userResp = await _client.PostAsJsonAsync("/api/users", userPayload);
        var user = await userResp.Content.ReadFromJsonAsync<UserResponse>();

        var storePayload = new { userId = user!.Id, name = "Loja", slug = SlugUnico() };
        var storeResp = await _client.PostAsJsonAsync("/api/stores", storePayload);
        var store = await storeResp.Content.ReadFromJsonAsync<StoreResponse>();

        return store!.Id;
    }

    [Fact]
    public async Task POST_ComDadosValidos_DeveRetornar201()
    {
        var storeId = await CriarLojaAtiva();
        var payload = new
        {
            storeId,
            name = "Notebook Dell",
            description = "Notebook para trabalho",
            price = 4500.00m,
            stock = 10
        };

        var response = await _client.PostAsJsonAsync("/api/products", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ProductResponse>();
        body!.Active.Should().BeTrue();
    }

    [Fact]
    public async Task POST_ComPrecoInvalido_DeveRetornar400()
    {
        var storeId = await CriarLojaAtiva();
        var payload = new
        {
            storeId,
            name = "Produto Válido",
            description = (string?)null,
            price = 0m,
            stock = 1
        };

        var response = await _client.PostAsJsonAsync("/api/products", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ComLojaInexistente_DeveRetornar404()
    {
        var payload = new
        {
            storeId = Guid.NewGuid(),
            name = "Produto Válido",
            description = (string?)null,
            price = 100m,
            stock = 1
        };

        var response = await _client.PostAsJsonAsync("/api/products", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_DeveDesativarProduto_ERetornar204()
    {
        var storeId = await CriarLojaAtiva();
        var createPayload = new
        {
            storeId,
            name = "Notebook Dell",
            description = (string?)null,
            price = 4500m,
            stock = 10
        };
        var createResp = await _client.PostAsJsonAsync("/api/products", createPayload);
        var produto = await createResp.Content.ReadFromJsonAsync<ProductResponse>();

        var response = await _client.DeleteAsync($"/api/products/{produto!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // produto não aparece mais na lista de ativos
        var listaResp = await _client.GetAsync("/api/products");
        var lista = await listaResp.Content.ReadFromJsonAsync<List<ProductResponse>>();
        lista.Should().NotContain(p => p.Id == produto.Id);
    }

    [Fact]
    public async Task PATCH_Activate_DeveReativarProduto()
    {
        var storeId = await CriarLojaAtiva();
        var createPayload = new
        {
            storeId,
            name = "Notebook Dell",
            description = (string?)null,
            price = 4500m,
            stock = 10
        };
        var createResp = await _client.PostAsJsonAsync("/api/products", createPayload);
        var produto = await createResp.Content.ReadFromJsonAsync<ProductResponse>();
        await _client.DeleteAsync($"/api/products/{produto!.Id}");

        var response = await _client.PatchAsync($"/api/products/{produto.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResp = await _client.GetAsync($"/api/products/{produto.Id}");
        var atualizado = await getResp.Content.ReadFromJsonAsync<ProductResponse>();
        atualizado!.Active.Should().BeTrue();
    }

    [Fact]
    public async Task PUT_ComDadosValidos_DeveRetornar200_EAtualizarUpdatedAt()
    {
        var storeId = await CriarLojaAtiva();
        var createPayload = new
        {
            storeId,
            name = "Notebook Dell",
            description = "Original",
            price = 4500m,
            stock = 10
        };
        var createResp = await _client.PostAsJsonAsync("/api/products", createPayload);
        var produto = await createResp.Content.ReadFromJsonAsync<ProductResponse>();

        var updatePayload = new
        {
            name = "Notebook Dell Inspiron",
            description = "Atualizado",
            price = 4299.90m,
            stock = 15
        };

        var response = await _client.PutAsJsonAsync($"/api/products/{produto!.Id}", updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ProductResponse>();
        body!.Name.Should().Be("Notebook Dell Inspiron");
        body.UpdatedAt.Should().NotBeNull();
    }
}