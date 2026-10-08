using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GenericStore.Application.DTOs.Stores;
using GenericStore.Application.DTOs.Users;

namespace GenericStore.IntegrationTests.Endpoints;

public class StoresEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public StoresEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static string SlugUnico() => $"loja-{Guid.NewGuid():N}";

    [Fact]
    public async Task POST_ComDadosValidos_DeveRetornar201_LojaAtiva()
    {
        var client = _factory.CreateClient();
        var login = await client.RegistrarELogarAsync();

        var payload = new { userId = login.UserId, name = "Minha Loja", slug = SlugUnico() };
        var response = await client.PostAsJsonAsync("/api/stores", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<StoreResponse>();
        body!.Active.Should().BeTrue();
    }

    [Fact]
    public async Task POST_ComUsuarioInexistente_DeveRetornar404()
    {
        var client = _factory.CreateClient();
        await client.RegistrarELogarAsync();

        var payload = new { userId = Guid.NewGuid(), name = "Minha Loja", slug = SlugUnico() };
        var response = await client.PostAsJsonAsync("/api/stores", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_ComSlugDuplicado_DeveRetornar409()
    {
        var client = _factory.CreateClient();
        var login = await client.RegistrarELogarAsync();

        var slug = SlugUnico();
        var payload = new { userId = login.UserId, name = "Loja", slug };

        await client.PostAsJsonAsync("/api/stores", payload);
        var response = await client.PostAsJsonAsync("/api/stores", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GET_LojasDoUsuario_DeveRetornar200()
    {
        var client = _factory.CreateClient();
        var login = await client.RegistrarELogarAsync();

        var payload = new { userId = login.UserId, name = "Loja A", slug = SlugUnico() };
        await client.PostAsJsonAsync("/api/stores", payload);

        var response = await client.GetAsync($"/api/users/{login.UserId}/stores");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<StoreResponse>>();
        body.Should().NotBeNull();
        body!.Count.Should().Be(1);
    }

    [Fact]
    public async Task GET_LojasDeUsuarioInexistente_DeveRetornar404()
    {
        var client = _factory.CreateClient();
        await client.RegistrarELogarAsync();

        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}/stores");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PATCH_Deactivate_DeveRetornar204()
    {
        var client = _factory.CreateClient();
        var login = await client.RegistrarELogarAsync();

        var payload = new { userId = login.UserId, name = "Loja", slug = SlugUnico() };
        var createResp = await client.PostAsJsonAsync("/api/stores", payload);
        var store = await createResp.Content.ReadFromJsonAsync<StoreResponse>();

        var response = await client.PatchAsync($"/api/stores/{store!.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResp = await client.GetAsync($"/api/stores/{store.Id}");
        var atualizada = await getResp.Content.ReadFromJsonAsync<StoreResponse>();
        atualizada!.Active.Should().BeFalse();
    }

    [Fact]
    public async Task PATCH_Activate_DeveRetornar204()
    {
        var client = _factory.CreateClient();
        var login = await client.RegistrarELogarAsync();

        var payload = new { userId = login.UserId, name = "Loja", slug = SlugUnico() };
        var createResp = await client.PostAsJsonAsync("/api/stores", payload);
        var store = await createResp.Content.ReadFromJsonAsync<StoreResponse>();
        await client.PatchAsync($"/api/stores/{store!.Id}/deactivate", null);

        var response = await client.PatchAsync($"/api/stores/{store.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResp = await client.GetAsync($"/api/stores/{store.Id}");
        var atualizada = await getResp.Content.ReadFromJsonAsync<StoreResponse>();
        atualizada!.Active.Should().BeTrue();
    }
}