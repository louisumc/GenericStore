using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GenericStore.Application.DTOs.Users;

namespace GenericStore.IntegrationTests.Endpoints;

public class UsersEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static string EmailUnico() => $"{Guid.NewGuid():N}@email.com";

    [Fact]
    public async Task POST_ComDadosValidos_DeveRetornar201()
    {
        var payload = new { name = "João Silva", email = EmailUnico() };

        var response = await _client.PostAsJsonAsync("/api/users", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<UserResponse>();
        body.Should().NotBeNull();
        body!.Name.Should().Be("João Silva");
    }

    [Fact]
    public async Task POST_ComEmailInvalido_DeveRetornar400()
    {
        var payload = new { name = "João Silva", email = "nao-eh-email" };

        var response = await _client.PostAsJsonAsync("/api/users", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ComEmailDuplicado_DeveRetornar409()
    {
        var email = EmailUnico();
        var payload = new { name = "João Silva", email };

        await _client.PostAsJsonAsync("/api/users", payload);
        var response = await _client.PostAsJsonAsync("/api/users", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GET_PorId_DeveRetornar200()
    {
        var payload = new { name = "Maria", email = EmailUnico() };
        var create = await _client.PostAsJsonAsync("/api/users", payload);
        var criado = await create.Content.ReadFromJsonAsync<UserResponse>();

        var response = await _client.GetAsync($"/api/users/{criado!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UserResponse>();
        body!.Email.Should().Be(payload.email);
    }

    [Fact]
    public async Task GET_PorIdInexistente_DeveRetornar404()
    {
        var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}