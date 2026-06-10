using GrpcStreamingDemo.Application.Interfaces;
using GrpcStreamingDemo.Domain.Entities;

namespace GrpcStreamingDemo.Application.Services;

public class UserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository repository, ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<User> CreateUserAsync(string name, string email, string role)
    {
        var user = new User { Name = name, Email = email, Role = role };
        var created = await _repository.AddAsync(user);
        _logger.LogInformation("User created with ID: {Id}", created.Id);
        return created;
    }
}