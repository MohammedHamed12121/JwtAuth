using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace JwtDemo;

public enum Roles
{
    User,
    Admin,
    Worker
}

public class AppUser
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    [JsonIgnore]
    public Roles Role { get; set; } = Roles.User;
}