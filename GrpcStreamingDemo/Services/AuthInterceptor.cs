using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace GrpcStreamingDemo.Services;

public class AuthInterceptor : Interceptor
{
    private readonly ILogger<AuthInterceptor> _logger;
    private const string ApiKeyHeader = "x-api-key";
    private const string AuthorizationHeader = "authorization";
    private const string SecretKey = "YourSuperSecretKeyForJwtDemo_AtLeast32Chars!"; // In production, read from appsettings

    public AuthInterceptor(ILogger<AuthInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        // API Key validation
        var apiKey = context.RequestHeaders.GetValue(ApiKeyHeader);
        if (!string.IsNullOrEmpty(apiKey) && apiKey == "my-secret-api-key-12345")
        {
            _logger.LogInformation("API Key authentication successful");
            return await continuation(request, context);
        }

        // JWT validation
        var authHeader = context.RequestHeaders.GetValue(AuthorizationHeader);
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (ValidateJwtToken(token))
            {
                _logger.LogInformation("JWT authentication successful");
                return await continuation(request, context);
            }
        }

        // Authentication failed
        _logger.LogWarning("Unauthorized access attempt");

        throw new RpcException(new Status(
            StatusCode.Unauthenticated,
            "Authentication failed. Please provide a valid API Key or JWT token."
        ));
    }

    private bool ValidateJwtToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(SecretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }
}