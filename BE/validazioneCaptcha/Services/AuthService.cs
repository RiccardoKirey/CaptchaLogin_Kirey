namespace validazioneCaptcha.Services;

public class AuthService : IAuthService
{
    private readonly string _validEmail;
    private readonly string _validPassword;

    public AuthService(IConfiguration configuration)
    {
        _validEmail = configuration["Auth:Email"] ?? string.Empty;
        _validPassword = configuration["Auth:Password"] ?? string.Empty;
    }

    public bool ValidateCredentials(string email, string password) =>
        !string.IsNullOrEmpty(_validEmail) &&
        !string.IsNullOrEmpty(_validPassword) &&
        email == _validEmail &&
        password == _validPassword;
}
