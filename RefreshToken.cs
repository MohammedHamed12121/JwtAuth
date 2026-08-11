namespace JwtDemo;

public class RefreshToken
{
    public string? UserEmail { get; set; }
    public string? RefToken { get; set; }
    public DateTime ExpiredAt {get; set;}
    public bool Revoked { get; set; }
}