namespace validazioneCaptcha.Services;

public class ReCaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private const string GoogleVerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

    public ReCaptchaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<bool> VerifyAsync(string token)
    {
        var secretKey = _configuration["ReCaptcha:SecretKey"];

        if (string.IsNullOrWhiteSpace(secretKey))
            return false;

        var formData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("secret", secretKey),
            new KeyValuePair<string, string>("response", token)
        ]);

        var response = await _httpClient.PostAsync(GoogleVerifyUrl, formData);

        if (!response.IsSuccessStatusCode)
            return false;

        var json = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<ReCaptchaResponse>(json);

        return result?.Success == true;
    }

    private class ReCaptchaResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}