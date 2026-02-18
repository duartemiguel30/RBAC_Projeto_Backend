using WorkplaceTasks.API.Models;

namespace WorkplaceTasks.API.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskItem>> GetAllTasksAsync(WorkplaceTasks.API.Models.TaskStatus? status, int page, int pageSize);
    Task<TaskItem?> GetTaskByIdAsync(Guid id);
    Task<TaskItem> CreateTaskAsync(TaskItem task);
    Task<bool> UpdateTaskAsync(Guid id, TaskItem task);
    Task<bool> DeleteTaskAsync(Guid id);
}