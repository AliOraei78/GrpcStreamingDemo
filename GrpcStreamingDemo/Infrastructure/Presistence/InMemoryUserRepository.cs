using GrpcStreamingDemo.Application.Interfaces;
using GrpcStreamingDemo.Domain.Entities;

namespace GrpcStreamingDemo.Infrastructure.Persistence;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private int _nextId = 1;

    public Task<User> AddAsync(User user)
    {
        user.Id = _nextId++;
        _users.Add(user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByIdAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    public Task<List<User>> GetAllAsync() => Task.FromResult(_users.ToList());

    public Task UpdateAsync(User user)
    {
        var existing = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existing != null)
        {
            existing.Name = user.Name;
            existing.Email = user.Email;
            existing.Role = user.Role;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user != null) _users.Remove(user);
        return Task.CompletedTask;
    }
}