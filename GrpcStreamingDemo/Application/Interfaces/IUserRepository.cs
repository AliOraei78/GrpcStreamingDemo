using GrpcStreamingDemo.Domain.Entities;

namespace GrpcStreamingDemo.Application.Interfaces;

public interface IUserRepository
{
    Task<User> AddAsync(User user);
    Task<User?> GetByIdAsync(int id);
    Task<List<User>> GetAllAsync();
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}