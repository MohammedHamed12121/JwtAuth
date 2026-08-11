using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using JwtDemo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Adding configuration for Auth 
builder.Services.Configure<AuthenticationSettings>(builder.Configuration.GetSection("Authentication"));



// Adding Token to the builder 
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddRoles();
builder.Services.AddSingleton<RefreshTokenService>();

builder.Services.AddAuthorizationBuilder();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();


app.MapGet("/", () =>
{
    return "Hello, World";
});


app.Map("/login", (AppUser request, RefreshTokenService tokenService) =>
{
    if(request.Email == "User@user.com" && request.Password == "Password")
    {
        request.Role = Roles.User;
    }
    else if(request.Email == "Admin@admin.com" && request.Password == "Password")
    {
        request.Role = Roles.Admin;
    }
    else if(request.Email == "Worker@worker.com" && request.Password == "Password")
    {
        request.Role = Roles.Worker;
    }
    else
    {
        return Results.Unauthorized();
    }


    
    var jwt = GenerateJwtToken(request.Email, request.Role);

    string refreshToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")));

    tokenService.Add(request.Email, request.Role, refreshToken,  DateTime.UtcNow.AddDays(7));
    return Results.Ok(new
    {
        access_token =jwt,
        refresh_token = refreshToken
    });
});

app.Map("/refresh", (RefreshRequest request, RefreshTokenService tokenService) =>
{
    var refreshToken = request.RefreshToken;
    var token = tokenService.Get(refreshToken);

    if (token is null || !tokenService.IsValid(refreshToken))
    {
        return Results.Unauthorized();
    }
    
    tokenService.Revoke(refreshToken);
    var newRefreshToken = Guid.NewGuid().ToString();
    tokenService.Add(token.UserEmail!, token.Role, newRefreshToken, DateTime.UtcNow.AddDays(7));

    var newAccessToken = GenerateJwtToken(token.UserEmail, token.Role);

    return Results.Ok(new 
    {
        accessToken = newAccessToken,
        refreshToken = newRefreshToken
    });
});

string GenerateJwtToken(string email, Roles role)
{
    var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("thisisthesecretforgeneratingakey(mustbeatleast32bitlong)"));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
    var claims = new []
    {
        new Claim(ClaimTypes.NameIdentifier, email),
        new Claim(ClaimTypes.Email, email),
        new Claim(ClaimTypes.Role, role.ToString())
    };
    Console.WriteLine(role.ToString());
    const int TokenLifetimeInSec = 200;

    AuthenticationSettings? authsetting = builder.Configuration.GetSection("Authentication").Get<AuthenticationSettings>(); 
    if(authsetting is null 
        || string.IsNullOrWhiteSpace(authsetting.Issuer)
        || string.IsNullOrWhiteSpace(authsetting.Audience)
        )
    {
        Results.InternalServerError();
    }

    return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
        issuer: authsetting!.Issuer,
        audience: authsetting!.Audience,
        claims: claims,
        expires: DateTime.UtcNow.AddSeconds(TokenLifetimeInSec),    
        signingCredentials: credentials));

}


app.MapGet("/public",
           () => "You can have access to a public resource");
app.MapGet("/public-secure",
           [Authorize]() => "You now have access to a public protected resource");
app.MapGet("/admin", () => "Hello Admin").RequireAuthorization(policy => policy.RequireRole("Admin"));
app.MapGet("/worker", () => "Hello worker").RequireAuthorization(policy => policy.RequireRole("Worker"));




app.Run();