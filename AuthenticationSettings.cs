using System.ComponentModel.DataAnnotations;

namespace JwtDemo;

// public record AuthenticationSettings(
//     string TokenSecret,
//     string RefreshTokenSecret,
//     string Issuer,
// //     string Audience
// );
public record AuthenticationSettings()
{
    public required string TokenSecret {get; set;}
    public required string RefreshTokenSecret {get; set;}
    public required string Issuer {get; set;}
    public required string Audience{get; set;}
}