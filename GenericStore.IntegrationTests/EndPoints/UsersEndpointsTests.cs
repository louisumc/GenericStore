using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GenericStore.Application.DTOs.Users;

namespace GenericStore.IntegrationTests.Endpoints;

public class UsersEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UsersEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static string EmailUnico() => $"{Guid.NewGuid():N}@email.com";

    [Fact]
    public async Task POST_ComDadosValidos_DeveRetornar201()
    {
        var payload = new { name = "João Silva", email = EmailUnico(), password = "senha123" };

        var response = await _client.PostAsJsonAsync("/api/users", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<UserResponse>();
        body.Should().NotBeNull();
        body!.Name.Should().Be("João Silva");
    }

    [Fact]
    public async Task POST_ComSenhaCurta_DeveRetornar400()
    {
        var payload = new { name = "João Silva", email = EmailUnico(), password = "123" };

        var response = await _client.PostAsJsonAsync("/api/users", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ComEmailInvalido_DeveRetornar400()
    {
        var payload = new { name = "João Silva", email = "nao-eh-email", password = "senha123" };

        var response = await _client.PostAsJsonAsync("/api/users", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ComEmailDuplicado_DeveRetornar409()
    {
        var email = EmailUnico();
        var payload = new { name = "João Silva", email, password = "senha123" };

        await _client.PostAsJsonAsync("/api/users", payload);
        var response = await _client.PostAsJsonAsync("/api/users", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GET_SemToken_DeveRetornar401()
    {
        var response = await _client.GetAsync("/api/users");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_ComToken_DeveRetornar200()
    {
        var authClient = _factory.CreateClient();
        await authClient.RegistrarELogarAsync();

        var response = await authClient.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_PorIdInexistente_DeveRetornar404()
    {
        var authClient = _factory.CreateClient();
        await authClient.RegistrarELogarAsync();

        var response = await authClient.GetAsync($"/api/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}