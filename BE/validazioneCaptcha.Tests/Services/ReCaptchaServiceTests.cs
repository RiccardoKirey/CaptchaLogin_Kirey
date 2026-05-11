using System.Net;
using Microsoft.Extensions.Configuration;
using validazioneCaptcha.Services;
using Xunit;

namespace validazioneCaptcha.Tests.Services;

public class ReCaptchaServiceTests
{
    private static ReCaptchaService CreateService(HttpResponseMessage httpResponse, string? secretKey = "test-secret")
    {
        var handler = new FakeHttpMessageHandler(httpResponse);
        var httpClient = new HttpClient(handler);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ReCaptcha:SecretKey"] = secretKey
            })
            .Build();

        return new ReCaptchaService(httpClient, config);
    }

    [Fact]
    public async Task VerifyAsync_WhenGoogleReturnsSuccess_ReturnsTrue()
    {
        var response = OkJson("""{"success":true}""");
        var service = CreateService(response);

        Assert.True(await service.VerifyAsync("valid-token"));
    }

    [Fact]
    public async Task VerifyAsync_WhenGoogleReturnsFailure_ReturnsFalse()
    {
        var response = OkJson("""{"success":false}""");
        var service = CreateService(response);

        Assert.False(await service.VerifyAsync("bad-token"));
    }

    [Fact]
    public async Task VerifyAsync_WhenHttpCallFails_ReturnsFalse()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var service = CreateService(response);

        Assert.False(await service.VerifyAsync("any-token"));
    }

    [Fact]
    public async Task VerifyAsync_WhenSecretKeyMissing_ReturnsFalse()
    {
        var response = OkJson("""{"success":true}""");
        var service = CreateService(response, secretKey: null);

        Assert.False(await service.VerifyAsync("any-token"));
    }

    private static HttpResponseMessage OkJson(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json) };
}

file sealed class FakeHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(response);
}
