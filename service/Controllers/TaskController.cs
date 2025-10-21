using Microsoft.AspNetCore.Mvc;
using service.Dto;
using service.Job;

namespace service.Controllers;

[ApiController]
[Route("task")]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TaskController> _logger;

    public TaskController(ITaskService taskService,
        ILogger<TaskController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    [HttpGet("ping")]
    public IActionResult CronPingTask()
    {
        _logger.LogInformation("CronPingTask executado!");
        return Ok($"Pong: {Environment.GetEnvironmentVariable("DBHOST")}");
    }

    [HttpPost("create")]
    public IActionResult CreateTask([FromBody] List<TaskRequest> tasks, CancellationToken cancellationToken)
    {
        //var result = _taskService.CreateTask(tasks, cancellationToken);
        var result = _taskService.CreateTaskNewAsync(tasks);

        if (!result.Success)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return StatusCode(result.StatusCode, new { data = result.Data });
    }

    [HttpPost("onoff")]
    public IActionResult OnTask(RequestJob request, CancellationToken cancellationToken)
    {
        var result = _taskService.OnOffTask(request, cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return StatusCode(result.StatusCode, new { data = result.Data });
    }

    [HttpDelete("delete")]
    public IActionResult DeleteTask([FromQuery] string recurringJobId)
    {
        var result = _taskService.DeleteTask(recurringJobId);

        if (!result.Success)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return StatusCode(result.StatusCode, new { data = result.Data });
    }
}

