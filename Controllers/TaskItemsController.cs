using Microsoft.AspNetCore.Mvc;
using WorkplaceTasks.API.Models;
using WorkplaceTasks.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace WorkplaceTasks.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TaskItemsController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TaskItemsController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    // GET: api/TaskItems?status=x&page=x&pageSize=x
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> Get(
    [FromQuery] WorkplaceTasks.API.Models.TaskStatus? status,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
        var tasks = await _taskService.GetAllTasksAsync(status, page, pageSize);
        return Ok(tasks);
    }

    // POST: api/TaskItems
    [HttpPost]
    public async Task<ActionResult<TaskItem>> Post(TaskItem task)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _taskService.CreateTaskAsync(task);
        return Ok(result);
    }

    // PUT: api/TaskItems/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(Guid id, TaskItem task)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var success = await _taskService.UpdateTaskAsync(id, task);
        return success ? NoContent() : Forbid();
    }

    // DELETE: api/TaskItems/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _taskService.DeleteTaskAsync(id);
        return success ? NoContent() : Forbid();
    }
}