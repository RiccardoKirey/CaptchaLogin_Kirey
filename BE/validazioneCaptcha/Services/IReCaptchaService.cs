namespace validazioneCaptcha.Services;

public interface IReCaptchaService
{
    Task<bool> VerifyAsync(string token);
}
