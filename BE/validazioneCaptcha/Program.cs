using validazioneCaptcha.Models;
using validazioneCaptcha.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddHttpClient<IReCaptchaService, ReCaptchaService>();
builder.Services.AddScoped<IAuthService, AuthService>();

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
    IReCaptchaService reCaptchaService,
    IAuthService authService) =>
{
    if (string.IsNullOrWhiteSpace(request.CaptchaToken))
        return Results.BadRequest(new LoginResponse { Ok = false, Message = "Token reCAPTCHA mancante." });

    var captchaValid = await reCaptchaService.VerifyAsync(request.CaptchaToken);
    if (!captchaValid)
        return Results.UnprocessableEntity(new LoginResponse { Ok = false, Message = "Verifica reCAPTCHA fallita. Riprova." });

    if (!authService.ValidateCredentials(request.Email, request.Password))
        return Results.Json(new LoginResponse { Ok = false, Message = "Credenziali non valide." }, statusCode: StatusCodes.Status401Unauthorized);

    return Results.Ok(new LoginResponse { Ok = true });
})
.Produces<LoginResponse>(StatusCodes.Status200OK)
.Produces<LoginResponse>(StatusCodes.Status400BadRequest)
.Produces<LoginResponse>(StatusCodes.Status401Unauthorized)
.Produces<LoginResponse>(StatusCodes.Status422UnprocessableEntity)
.WithName("Login")
.WithTags("Auth");

app.Run();

//serve per test
public partial class Program { }
