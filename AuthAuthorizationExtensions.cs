namespace JwtDemo;

public static class AuthAuthorizationExtensions
{
    public static void AddRoles(this IServiceCollection service)
    {
        service.AddAuthorization(options =>
        {
            options.AddPolicy("AdminPolicy", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("WorkerPolicy", policy =>
                policy.RequireRole("Worker"));
        });
    }
}