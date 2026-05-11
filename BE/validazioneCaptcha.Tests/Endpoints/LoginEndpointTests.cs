using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using validazioneCaptcha.Models;
using validazioneCaptcha.Services;
using Xunit;

namespace validazioneCaptcha.Tests.Endpoints;

public class LoginEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LoginEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient(bool captchaValid, bool credentialsValid)
    {
        var captchaMock = new Mock<IReCaptchaService>();
        captchaMock.Setup(s => s.VerifyAsync(It.IsAny<string>())).ReturnsAsync(captchaValid);

        var authMock = new Mock<IAuthService>();
        authMock.Setup(s => s.ValidateCredentials(It.IsAny<string>(), It.IsAny<string>())).Returns(credentialsValid);

        return _factory.WithWebHostBuilder(b =>
            b.ConfigureServices(services =>
            {
                services.AddSingleton(captchaMock.Object);
                services.AddSingleton(authMock.Object);
            })
        ).CreateClient();
    }

    [Fact]
    public async Task Login_WithMissingCaptchaToken_Returns400()
    {
        var client = CreateClient(captchaValid: true, credentialsValid: true);
        var body = new LoginRequest { Email = "admin@kirey.com", Password = "password123", CaptchaToken = null };

        var response = await client.PostAsJsonAsync("/api/auth/login", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCaptcha_Returns422()
    {
        var client = CreateClient(captchaValid: false, credentialsValid: true);
        var body = new LoginRequest { Email = "admin@kirey.com", Password = "password123", CaptchaToken = "bad" };

        var response = await client.PostAsJsonAsync("/api/auth/login", body);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongCredentials_Returns401()
    {
        var client = CreateClient(captchaValid: true, credentialsValid: false);
        var body = new LoginRequest { Email = "wrong@kirey.com", Password = "wrong", CaptchaToken = "valid" };

        var response = await client.PostAsJsonAsync("/api/auth/login", body);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidRequest_Returns200WithOkTrue()
    {
        var client = CreateClient(captchaValid: true, credentialsValid: true);
        var body = new LoginRequest { Email = "admin@kirey.com", Password = "password123", CaptchaToken = "valid" };

        var response = await client.PostAsJsonAsync("/api/auth/login", body);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.True(result.Ok);
    }
}
