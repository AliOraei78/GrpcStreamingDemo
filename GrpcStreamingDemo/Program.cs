using GrpcStreamingDemo.Application.Interfaces;
using GrpcStreamingDemo.Application.Services;
using GrpcStreamingDemo.Infrastructure.Persistence;
using GrpcStreamingDemo.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Dependency Injection
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddScoped<UserService>();
// Register interceptors in DI container
builder.Services.AddScoped<LoggingInterceptor>();
builder.Services.AddScoped<ValidationInterceptor>();
builder.Services.AddScoped<AuthInterceptor>();

builder.Services.AddHealthChecks()
    .AddCheck<GrpcHealthCheck>("grpc_health_check", HealthStatus.Degraded, new[] { "grpc" });

// Register gRPC and configure specific options for GreeterService
builder.Services.AddGrpc()
    .AddServiceOptions<GreeterService>(options =>
    {
        options.Interceptors.Add<LoggingInterceptor>();
        //options.Interceptors.Add<ValidationInterceptor>();
       // options.Interceptors.Add<AuthInterceptor>();
    });

builder.Services.AddLogging();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapHealthChecks("/healthz", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("grpc")
});

// 2. Map the service cleanly
app.MapGrpcService<GreeterService>();

app.MapGet("/", () => "gRPC service is running. Use a gRPC client to call it.");

app.Run();