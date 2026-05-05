using validazioneCaptcha.Models;
using validazioneCaptcha.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddHttpClient<ReCaptchaService>();
builder.Services.AddScoped<ReCaptchaService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5500")
              .AllowAnyHeader()
              .WithMethods("POST");
    });
});

var app = builder.Build();

app.UseCors("FrontendPolicy");

var api = app.MapGroup("/api");

api.MapPost("/auth/login", async (
    LoginRequest request,
    ReCaptchaService reCaptchaService) =>
{
    if (string.IsNullOrWhiteSpace(request.CaptchaToken))
    {
        return Results.Ok(new LoginResponse
        {
            Ok = false,
            Message = "Token reCAPTCHA mancante."
        });
    }

    var captchaValid = await reCaptchaService.VerifyAsync(request.CaptchaToken);

    if (!captchaValid)
    {
        return Results.Ok(new LoginResponse
        {
            Ok = false,
            Message = "Verifica reCAPTCHA fallita. Riprova."
        });
    }

    var emailValida = request.Email == "admin@kirey.com";
    var passwordValida = request.Password == "password123";

    if (!emailValida || !passwordValida)
    {
        return Results.Ok(new LoginResponse
        {
            Ok = false,
            Message = "Credenziali non valide."
        });
    }

    return Results.Ok(new LoginResponse { Ok = true });
})
.Produces<LoginResponse>(StatusCodes.Status200OK)
.WithName("Login")
.WithTags("Auth");

app.Run();