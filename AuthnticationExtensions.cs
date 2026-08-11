
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace JwtDemo;

public static class AuthAuthenticationExtensions
{
    public static void AddJwtAuth(this IServiceCollection service, IConfiguration config)
    {
        var authSettings = config.GetSection("Authentication").Get<AuthenticationSettings>();
        if(authSettings is null 
            || string.IsNullOrWhiteSpace(authSettings.Issuer)
            || string.IsNullOrWhiteSpace(authSettings.Audience)
            )
        {
            Results.InternalServerError();
        }

        service.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = authSettings!.Issuer,
                    ValidAudience = authSettings!.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("thisisthesecretforgeneratingakey(mustbeatleast32bitlong)")),
                    ClockSkew = TimeSpan.Zero
                };
        });
    }
}