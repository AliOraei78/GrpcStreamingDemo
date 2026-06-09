using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GrpcStreamingDemo.Services;

public class GrpcHealthCheck : IHealthCheck
{
    private readonly ILogger<GrpcHealthCheck> _logger;

    public GrpcHealthCheck(ILogger<GrpcHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // You can add real checks here (database connection, memory usage, etc.)
        var isHealthy = true; // For demo purposes always healthy

        if (isHealthy)
        {
            _logger.LogInformation("Health check passed");
            return Task.FromResult(
                HealthCheckResult.Healthy("gRPC service is healthy and ready."));
        }

        return Task.FromResult(
            HealthCheckResult.Unhealthy("gRPC service is experiencing issues."));
    }
}