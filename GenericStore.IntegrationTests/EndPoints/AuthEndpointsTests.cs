using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GenericStore.Application.DTOs.Auth;

namespace GenericStore.IntegrationTests.Endpoints;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static string EmailUnico() => $"{Guid.NewGuid():N}@email.com";

    [Fact]
    public async Task POST_Login_ComCredenciaisValidas_DeveRetornar200_E_Token()
    {
        var client = _factory.CreateClient();
        var email = EmailUnico();
        await client.PostAsJsonAsync("/api/users", new { name = "João", email, password = "senha123" });

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "senha123" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.Email.Should().Be(email);
    }

    [Fact]
    public async Task POST_Login_ComSenhaErrada_DeveRetornar400()
    {
        var client = _factory.CreateClient();
        var email = EmailUnico();
        await client.PostAsJsonAsync("/api/users", new { name = "João", email, password = "senha123" });

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "errada" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Login_ComEmailInexistente_DeveRetornar400()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email = EmailUnico(), password = "senha123" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}