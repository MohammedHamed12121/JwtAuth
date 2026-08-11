namespace JwtDemo;

public class RefreshToken
{
    public string? UserEmail { get; set; }
    public Roles Role { get; set; } = Roles.User;
    public string? RefToken { get; set; }
    public DateTime ExpiredAt {get; set;}
    public bool Revoked { get; set; }
}