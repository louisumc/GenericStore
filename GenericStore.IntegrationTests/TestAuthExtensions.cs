using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenericStore.Application.DTOs.Auth;
using GenericStore.Application.DTOs.Users;

namespace GenericStore.IntegrationTests;

public static class TestAuthExtensions
{
    public static async Task<LoginResponse> RegistrarELogarAsync(
        this HttpClient client,
        string? email = null,
        string password = "senha123")
    {
        email ??= $"{Guid.NewGuid():N}@email.com";

        var createPayload = new { name = "Usuário Teste", email, password };
        var createResp = await client.PostAsJsonAsync("/api/users", createPayload);
        createResp.EnsureSuccessStatusCode();

        var loginPayload = new { email, password };
        var loginResp = await client.PostAsJsonAsync("/api/auth/login", loginPayload);
        loginResp.EnsureSuccessStatusCode();

        var login = await loginResp.Content.ReadFromJsonAsync<LoginResponse>()
            ?? throw new InvalidOperationException("Falha ao logar.");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.Token);

        return login;
    }
}