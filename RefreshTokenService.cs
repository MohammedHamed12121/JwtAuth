using System.Collections.Concurrent;

namespace JwtDemo;

public class RefreshTokenService
{
    private readonly ConcurrentDictionary<string, RefreshToken> _tokens = new();

    public RefreshToken Add(string email, string token, DateTime expireAt)
    {
        var refreshToken = new RefreshToken
        {
            UserEmail = email,
            RefToken = token,
            ExpiredAt = expireAt,
            Revoked = false
        };

        _tokens[token] = refreshToken;
        return refreshToken;
    }

    public RefreshToken? Get(string refreshToken)
    {
        _tokens.TryGetValue(refreshToken, out var token);
        return token;
    }

    public bool Revoke(string refreshToken)
    {
        if(_tokens.TryGetValue(refreshToken, out var token))
        {
            token.Revoked = true;
            return true;
        }
        return false;
    }

    public void RevokeForUser(string email)
    {
        foreach(var token in _tokens.Values.Where(t => t.UserEmail == email))
        {
            token.Revoked = true;
        }
    }

    public bool IsValid(string refreshToken)
    {
        var token = Get(refreshToken);
        return token is not null 
                && !token.Revoked
                && token.ExpiredAt > DateTime.UtcNow;
    }
}