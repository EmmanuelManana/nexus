using Microsoft.AspNetCore.Mvc;
using Nexus.Api.Helpers;
using Nexus.Api.Validation;
using Nexus.Application.Contracts;
using Nexus.Application.Dtos;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/tasks?q=search&amp;sort=dueDate:asc|dueDate:desc
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetTasks(
        [FromQuery] string? q,
        [FromQuery] string? sort,
        CancellationToken cancellationToken)
    {
        var sortDueDateAsc = SortHelper.ParseDueDateSort(sort);
        var tasks = await _taskService.GetTasksAsync(q, sortDueDateAsc, cancellationToken);
        return Ok(tasks);
    }

    /// <summary>
    /// GET /api/tasks/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return NotFound(ProblemDetailsHelper.For(404, "Not Found", $"Task with id {id} was not found.", id));
        return Ok(task);
    }

    /// <summary>
    /// POST /api/tasks
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponse>> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var errors = TaskRequestValidator.ValidateCreate(request);
        if (errors.Count > 0)
            return BadRequest(ProblemDetailsHelper.ForValidation(400, "Validation failed", errors));

        var created = await _taskService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// PUT /api/tasks/{id}
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> Update(int id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var errors = TaskRequestValidator.ValidateUpdate(request);
        if (errors.Count > 0)
            return BadRequest(ProblemDetailsHelper.ForValidation(400, "Validation failed", errors));

        var updated = await _taskService.UpdateAsync(id, request, cancellationToken);
        if (updated is null)
            return NotFound(ProblemDetailsHelper.For(404, "Not Found", $"Task with id {id} was not found.", id));
        return Ok(updated);
    }

    /// <summary>
    /// DELETE /api/tasks/{id}
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _taskService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(ProblemDetailsHelper.For(404, "Not Found", $"Task with id {id} was not found.", id));
        return NoContent();
    }
}
