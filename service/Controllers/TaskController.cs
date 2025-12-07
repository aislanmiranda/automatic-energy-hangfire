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
        var result = _taskService.CreateTaskNewAsync(tasks, cancellationToken);

        if (!result.Result.Success)
            return StatusCode(result.Result.StatusCode, new { error = result.Result.Error });

        return StatusCode(result.Result.StatusCode, new { data = result.Result.Data });
    }

    [HttpPost("onoff")]
    public IActionResult OnTask(TaskRequest request, CancellationToken cancellationToken)
    {
        var response = _taskService.OnOffTaskAsync(request, cancellationToken);

        if (!response.Result.Success)
            return StatusCode(response.Result.StatusCode, new { error = response.Result.Error });

        return StatusCode(response.Result.StatusCode, new { data = response.Result.Data });
    }

    [HttpDelete("delete")]
    public IActionResult DeleteTask([FromQuery] string recurringJobId)
    {
        var response = _taskService.DeleteTaskAsync(recurringJobId);

        if (!response.Result.Success)
            return StatusCode(response.Result.StatusCode, new { error = response.Result.Error });

        return StatusCode(response.Result.StatusCode, new { data = response.Result.Data });
    }

 }

