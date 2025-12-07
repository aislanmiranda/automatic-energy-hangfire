using Quartz;
using service.Dto;
using service.Queue;
using Service.Dto;

namespace service.Job;

public class TaskService : ITaskService
{
    private const string TIMEZONE_ID = "E. South America Standard Time";
    private readonly IScheduler _scheduler;
    private readonly IRabbitMQService _rabbit;

    public TaskService(IScheduler scheduler, IRabbitMQService rabbit)
    {
        _scheduler = scheduler;
        _rabbit = rabbit;
    }
   
    public async Task<Result<List<TaskRequest>>> CreateTaskNewAsync(List<TaskRequest> tasks, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var task in tasks)
            {
                var jobKey = new JobKey(task.TaskJobId);

                await _scheduler.DeleteJob(jobKey);

                var job = JobBuilder.Create<SendMessageQueueJob>()
                    .WithIdentity(jobKey)
                    .UsingJobData(new JobDataMap
                    {
                        ["Request"] = task,
                        ["CancellationToken"] = cancellationToken
                    })
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity($"{task.TaskJobId}")
                    .WithCronSchedule(task.Expression,
                        x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(TIMEZONE_ID)))
                    .StartNow()
                .Build();

                await _scheduler.ScheduleJob(job, trigger);
            }

            return Result<List<TaskRequest>>.Create(tasks);
        }
        catch (Exception ex)
        {
            return Result<List<TaskRequest>>.Fail($"Erro ao registrar jobs: {ex.Message}");
        }
    }

    public async Task<Result<string>> OnOffTaskAsync(TaskRequest request, CancellationToken cancellationToken)
    {
        try
        {
            string MESSAGE_RESULT = $"Task {request.Action.ToUpper()} criada com sucesso.";

            // Cria um job único
            var job = JobBuilder.Create<SendMessageQueueJob>()
                .WithIdentity($"instant-job-{Guid.NewGuid()}")
                .UsingJobData(new JobDataMap
                {
                    ["Request"] = request,
                    ["CancellationToken"] = cancellationToken
                })
                .Build();

            // Cria um trigger para executar imediatamente
            var trigger = TriggerBuilder.Create()
                .StartNow()
                .Build();

            // Agenda e executa o job imediatamente
            await _scheduler.ScheduleJob(job, trigger, cancellationToken);

            return Result<string>.Ok(MESSAGE_RESULT);
        }
        catch (Exception ex)
        {
            return Result<string>.Fail($"Erro ao executar OnOffTask: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteTaskAsync(string recurringJobId)
    {
        try
        {
            var jobKey = new JobKey($"{recurringJobId}", "DEFAULT");

            if (await _scheduler.CheckExists(jobKey))
            {
                await _scheduler.DeleteJob(jobKey);
            }

            return Result<string>.Ok("Task removida com sucesso.");
        }
        catch (Exception)
        {
            return Result<string>.Fail("Falha ao remover task.");
        }
    }

}