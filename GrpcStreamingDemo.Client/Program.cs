using Grpc.Core;
using Grpc.Net.Client;
using GrpcStreamingDemo;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

Console.WriteLine("=== gRPC Authentication Demo - Day 12 ===\n");

using var channel = GrpcChannel.ForAddress("https://localhost:7007");
var client = new Greeter.GreeterClient(channel);

async Task TestWithHeaders(Metadata headers, string testName)
{
    try
    {
        Console.WriteLine($"\nAuthentication Test: {testName}");

        var reply = await client.SayHelloAsync(
            new HelloRequest { Name = "Ali Jenabi" },
            headers: headers
        );

        Console.WriteLine($"Success: {reply.Message}");
    }
    catch (RpcException ex) when (ex.StatusCode == StatusCode.Unauthenticated)
    {
        Console.WriteLine($"Unauthorized: {ex.Status.Detail}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

// Test 1: No authentication (should fail)
await TestWithHeaders(new Metadata(), "No Authentication");

// Test 2: API Key authentication
var apiKeyHeaders = new Metadata { { "x-api-key", "my-secret-api-key-12345" } };
await TestWithHeaders(apiKeyHeaders, "Valid API Key");

// Test 3: JWT authentication
var jwtToken = GenerateJwtToken();
var jwtHeaders = new Metadata { { "authorization", $"Bearer {jwtToken}" } };
await TestWithHeaders(jwtHeaders, "Valid JWT");

Console.WriteLine("\nAuthentication tests completed.");
Console.ReadKey();

static string GenerateJwtToken()
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes("YourSuperSecretKeyForJwtDemo_AtLeast32Chars!");

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim("sub", "Ali Jenabi")
        }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}