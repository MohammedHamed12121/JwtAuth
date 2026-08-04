using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using JwtDemo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Adding configuration for Auth 
builder.Services.Configure<AuthenticationSettings>(builder.Configuration.GetSection("Authentication"));



// Adding Token to the builder 
builder.Services.AddJwtAuth(builder.Configuration);

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


app.Map("/token", (LoginRequest request) =>
{
    if(request.Email != "User@user.com" && request.Password != "Password")  
        return Results.Unauthorized();

    var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("thisisthesecretforgeneratingakey(mustbeatleast32bitlong)"));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
    var claims = new []
    {
        new Claim(JwtRegisteredClaimNames.Sub, request.Email),
        new Claim(JwtRegisteredClaimNames.Email, request.Email)
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

    var jwt = new JwtSecurityToken(
        issuer: authsetting!.Issuer,
        audience: authsetting!.Audience,
        claims: claims,
        expires: DateTime.UtcNow.AddSeconds(TokenLifetimeInSec),    
        signingCredentials: credentials);

    return Results.Ok(new
    {
        access_token = new JwtSecurityTokenHandler().WriteToken(jwt),
        expires_in = TokenLifetimeInSec
    });
});

app.MapGet("/secure",
           [Authorize]() => "You now have access to a protected resource");



app.Run();