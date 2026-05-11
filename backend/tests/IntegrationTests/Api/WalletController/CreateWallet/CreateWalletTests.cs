using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Domain.Entities;
using IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Api.WalletController.CreateWallet;

public class CreateWalletTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CreateWalletTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    private HttpClient CreateAuthenticatedClientAsync(long userId, string role)
    {
        var token = GenerateTestJwtToken(userId, role);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static string GenerateTestJwtToken(long userId, string role)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("YourSuperSecretKeyShouldBeAtLeast32CharactersLong!");
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var token = handler.CreateToken(new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim("sub", userId.ToString()),
                new System.Security.Claims.Claim("role", role)
            }),
            SigningCredentials = credentials,
            Issuer = "MicroWallet",
            Audience = "MicroWalletUsers"
        });

        return handler.WriteToken(token);
    }

    private async Task SeedUserAsync(long userId, string email)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User
        {
            Id = userId,
            Email = email,
            Password = "hashed_password",
            Provider = Domain.Enums.AuthProvider.Local,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task POST_Wallet_WithValidRequest_ShouldReturn200Ok()
    {
        const long testUserId = 1;
        await SeedUserAsync(testUserId, "test1@example.com");
        var client = CreateAuthenticatedClientAsync(testUserId, "User");

        var requestBody = new StringContent("{\"currency\":\"THB\"}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/wallet", requestBody);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task POST_Wallet_WithValidRequest_ShouldReturnWalletResponse()
    {
        const long testUserId = 2;
        await SeedUserAsync(testUserId, "test2@example.com");
        var client = CreateAuthenticatedClientAsync(testUserId, "User");

        var requestBody = new StringContent("{\"currency\":\"USD\"}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/wallet", requestBody);

        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("walletId", content);
        Assert.Contains("walletNumber", content);
        Assert.Contains("balance", content);
        Assert.Contains("USD", content);
    }

    [Fact]
    public async Task POST_Wallet_WithoutAuth_ShouldReturn401Unauthorized()
    {
        var requestBody = new StringContent("{\"currency\":\"THB\"}", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/wallet", requestBody);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task POST_Wallet_WithInvalidCurrency_ShouldReturn400BadRequest()
    {
        const long testUserId = 3;
        await SeedUserAsync(testUserId, "test3@example.com");
        var client = CreateAuthenticatedClientAsync(testUserId, "User");

        var requestBody = new StringContent("{\"currency\":\"EUR\"}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/wallet", requestBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_Wallet_WithEmptyCurrency_ShouldReturn400BadRequest()
    {
        const long testUserId = 4;
        await SeedUserAsync(testUserId, "test4@example.com");
        var client = CreateAuthenticatedClientAsync(testUserId, "User");

        var requestBody = new StringContent("{\"currency\":\"\"}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/wallet", requestBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}