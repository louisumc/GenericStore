using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GenericStore.Application.DTOs.Stores;
using GenericStore.Application.DTOs.Users;

namespace GenericStore.IntegrationTests.Endpoints;

public class StoresEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public StoresEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static string EmailUnico() => $"{Guid.NewGuid():N}@email.com";
    private static string SlugUnico() => $"loja-{Guid.NewGuid():N}";

    private async Task<Guid> CriarUsuario()
    {
        var payload = new { name = "Owner", email = EmailUnico() };
        var response = await _client.PostAsJsonAsync("/api/users", payload);
        var body = await response.Content.ReadFromJsonAsync<UserResponse>();
        return body!.Id;
    }

    [Fact]
    public async Task POST_ComDadosValidos_DeveRetornar201_LojaAtiva()
    {
        var userId = await CriarUsuario();
        var payload = new { userId, name = "Minha Loja", slug = SlugUnico() };

        var response = await _client.PostAsJsonAsync("/api/stores", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<StoreResponse>();
        body!.Active.Should().BeTrue();
    }

    [Fact]
    public async Task POST_ComUsuarioInexistente_DeveRetornar404()
    {
        var payload = new { userId = Guid.NewGuid(), name = "Minha Loja", slug = SlugUnico() };

        var response = await _client.PostAsJsonAsync("/api/stores", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_ComSlugDuplicado_DeveRetornar409()
    {
        var userId = await CriarUsuario();
        var slug = SlugUnico();
        var payload = new { userId, name = "Loja", slug };

        await _client.PostAsJsonAsync("/api/stores", payload);
        var response = await _client.PostAsJsonAsync("/api/stores", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GET_LojasDoUsuario_DeveRetornar200()
    {
        var userId = await CriarUsuario();
        var payload = new { userId, name = "Loja A", slug = SlugUnico() };
        await _client.PostAsJsonAsync("/api/stores", payload);

        var response = await _client.GetAsync($"/api/users/{userId}/stores");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<StoreResponse>>();
        body.Should().NotBeNull();
        body!.Count.Should().Be(1);
    }

    [Fact]
    public async Task GET_LojasDeUsuarioInexistente_DeveRetornar404()
    {
        var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}/stores");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}