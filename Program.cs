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


app.Map("/login", (LoginRequest request, RefreshTokenService tokenService) =>
{
    if(request.Email != "User@user.com" && request.Password != "Password")  
        return Results.Unauthorized();

    
    var jwt = GenerateJwtToken(request.Email);

    string refreshToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")));

    tokenService.Add(request.Email, refreshToken,  DateTime.UtcNow.AddDays(7));
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
    tokenService.Add(token.UserEmail!, newRefreshToken, DateTime.UtcNow.AddDays(7));

    var newAccessToken = GenerateJwtToken(token.UserEmail);

    return Results.Ok(new 
    {
        accessToken = newAccessToken,
        refreshToken = newRefreshToken
    });
});

string GenerateJwtToken(string email)
{
    var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("thisisthesecretforgeneratingakey(mustbeatleast32bitlong)"));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
    var claims = new []
    {
        new Claim(JwtRegisteredClaimNames.Sub, email),
        new Claim(JwtRegisteredClaimNames.Email, email)
    };

    const int TokenLifetimeInSec = 20;

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
app.MapGet("/secure",
           [Authorize]() => "You now have access to a protected resource");



app.Run();