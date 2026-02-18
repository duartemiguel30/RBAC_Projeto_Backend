using Microsoft.EntityFrameworkCore;
using WorkplaceTasks.API.Data;
using WorkplaceTasks.API.Models;

namespace WorkplaceTasks.API.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> RegisterAsync(User user, string password)
    {
        // Forçamos a role "member" conforme as regras de segurança do enunciado [cite: 6]
        user.Role = "member";
        // Guardamos o texto limpo conforme pediste (sem hashing)
        user.PasswordHash = password;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        // O Admin precisa disto para "gerir usuários" 
        return await _context.Users.ToListAsync();
    }

    public async Task<bool> UpdateUserRoleAsync(Guid userId, string newRole)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        // O Admin pode "atribuir roles" 
        user.Role = newRole.ToLower();
        await _context.SaveChangesAsync();
        return true;
    }
}