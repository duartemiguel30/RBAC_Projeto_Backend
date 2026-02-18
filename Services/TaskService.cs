using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkplaceTasks.API.Data;
using WorkplaceTasks.API.Models;

namespace WorkplaceTasks.API.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TaskService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    private (string Id, string Role) GetCurrentUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
        var role = user?.FindFirstValue(ClaimTypes.Role)?.ToLower() ?? "member";
        return (id, role);
    }

    // GET /tasks (filtro)
    public async Task<IEnumerable<TaskItem>> GetAllTasksAsync(
    WorkplaceTasks.API.Models.TaskStatus? status,
    int page,
    int pageSize)
    {
        var (userId, userRole) = GetCurrentUser();
        var query = _context.Tasks.AsQueryable();

        // Admin e Manager veem tudo. Member vê apenas as suas tasks
        if (userRole != "admin" && userRole != "manager")
        {
            query = query.Where(t => t.CreatedByUserId == userId);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        // Paginação
        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetTaskByIdAsync(Guid id) => await _context.Tasks.FindAsync(id);

    // POST /tasks 
    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        var (userId, _) = GetCurrentUser();

        task.Id = Guid.NewGuid();
        task.CreatedAt = DateTime.UtcNow; // Timestamp de criação 
        task.UpdatedAt = DateTime.UtcNow;
        task.CreatedByUserId = userId;

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    // PUT /tasks/{id} 
    public async Task<bool> UpdateTaskAsync(Guid id, TaskItem task)
    {
        var (userId, userRole) = GetCurrentUser();
        var existingTask = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);

        if (existingTask == null) return false;

        // Admin/Manager editam qualquer uma. Member apenas a sua
        if (userRole != "admin" && userRole != "manager" && existingTask.CreatedByUserId != userId)
        {
            return false;
        }

        task.Id = id;
        task.CreatedAt = existingTask.CreatedAt; 
        task.UpdatedAt = DateTime.UtcNow;        
        task.CreatedByUserId = existingTask.CreatedByUserId;

        _context.Entry(task).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return true;
    }

    // DELETE /tasks/{id} 
    public async Task<bool> DeleteTaskAsync(Guid id)
    {
        var (userId, userRole) = GetCurrentUser();
        var task = await _context.Tasks.FindAsync(id);

        if (task == null) return false;

        // Apenas Admin apaga tudo. Manager/Member só as suas
        if (userRole != "admin" && task.CreatedByUserId != userId)
        {
            return false;
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }
}