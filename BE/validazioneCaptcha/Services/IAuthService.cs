namespace validazioneCaptcha.Services;

public interface IAuthService
{
    bool ValidateCredentials(string email, string password);
}
