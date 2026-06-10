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

    public async Task<User> CreateUserAsync(string name, string email, string? role = null)
    {
        var user = new User
        {
            Name = name,
            Email = email,
            Role = role ?? "User"
        };

        var createdUser = await _repository.AddAsync(user);
        _logger.LogInformation("User created successfully. ID: {Id}", createdUser.Id);
        return createdUser;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        await _repository.UpdateAsync(user);
        _logger.LogInformation("User updated. ID: {Id}", user.Id);
    }

    public async Task DeleteUserAsync(int id)
    {
        await _repository.DeleteAsync(id);
        _logger.LogInformation("User deleted. ID: {Id}", id);
    }
}