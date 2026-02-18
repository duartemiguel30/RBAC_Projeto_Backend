using WorkplaceTasks.API.Models;

namespace WorkplaceTasks.API.Services;

public interface IUserService
{
    Task<User> RegisterAsync(User user, string password);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<bool> UpdateUserRoleAsync(Guid userId, string newRole);
}