using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Adding Token to the builder 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // ValidIssuer would be based on your IdP
            ValidIssuer = "https://yourissuer.example",
            // ValidAudience would be based on your IdP
            ValidAudience = "https://youraudience.example",
            // IssuerSigningKey would not be specified if using an IdP
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("thisisthesecretforgeneratingakey(mustbeatleast32bitlong)")),
            ClockSkew = TimeSpan.Zero
        };
});
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

    const int TokenLifetimeMinutes = 20;    //Hardcoded for demo purposes
    var jwt = new JwtSecurityToken(
        issuer: "https://yourissuer.example",
        audience: "https://youraudience.example",
        claims: claims,
        //Typical short lifetime used with JWTs
        expires: DateTime.UtcNow.AddSeconds(TokenLifetimeMinutes),    
        signingCredentials: credentials);

    return Results.Ok(new
    {
        access_token = new JwtSecurityTokenHandler().WriteToken(jwt),
        expires_in = TokenLifetimeMinutes
    });
});

app.MapGet("/secure",
           [Authorize]() => "You now have access to a protected resource");



app.Run();