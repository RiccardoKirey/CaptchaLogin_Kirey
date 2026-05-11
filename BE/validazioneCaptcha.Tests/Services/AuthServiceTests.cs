using Microsoft.Extensions.Configuration;
using validazioneCaptcha.Services;
using Xunit;

namespace validazioneCaptcha.Tests.Services;

public class AuthServiceTests
{
    private static AuthService CreateService(string email = "admin@kirey.com", string password = "password123")
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:Email"] = email,
                ["Auth:Password"] = password
            })
            .Build();

        return new AuthService(config);
    }

    [Fact]
    public void ValidateCredentials_WithCorrectCredentials_ReturnsTrue()
    {
        var service = CreateService();
        Assert.True(service.ValidateCredentials("admin@kirey.com", "password123"));
    }

    [Fact]
    public void ValidateCredentials_WithWrongPassword_ReturnsFalse()
    {
        var service = CreateService();
        Assert.False(service.ValidateCredentials("admin@kirey.com", "wrongpassword"));
    }

    [Fact]
    public void ValidateCredentials_WithWrongEmail_ReturnsFalse()
    {
        var service = CreateService();
        Assert.False(service.ValidateCredentials("other@kirey.com", "password123"));
    }

    [Fact]
    public void ValidateCredentials_WithEmptyConfig_ReturnsFalse()
    {
        var config = new ConfigurationBuilder().Build();
        var service = new AuthService(config);
        Assert.False(service.ValidateCredentials(string.Empty, string.Empty));
    }
}
