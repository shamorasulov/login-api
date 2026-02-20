using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- THE INGREDIENTS (Services) ---

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        // Simple check to prevent the 'possibly null' warning
        if (document.Components != null && document.Components.SecuritySchemes != null)
        {
            // We removed '.Models' because your environment is forcing the 3.x structure
            var scheme = new Microsoft.OpenApi.OpenApiSecurityScheme
            {
                Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            };

            document.Components.SecuritySchemes.Add("Bearer", scheme);
        }
        return Task.CompletedTask;
    });
});

var secretKey = Encoding.ASCII.GetBytes("STRICTLY_SECRET_KEY_FOR_INTERVIEW_123");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // This makes the JSON document available
}

// --- THE PIPELINE (Order) ---

app.UseAuthentication();
app.UseAuthorization();

// --- THE ENDPOINTS ---

// 1. MOCK LOGIN: Run this to get your "Passport" (Token)
app.MapPost("/login", () =>
{
    var claims = new[] { new Claim(ClaimTypes.Name, "User123"), new Claim(ClaimTypes.Role, "Admin") };
    var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().CreateToken(new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
    });
    return Results.Ok(new { token = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token) });
});

// 2. PROTECTED DATA: You can't see this without the token
app.MapGet("/weatherforecast", () => new[] { "Chilly", "Hot" })
   .RequireAuthorization();

app.Run();