using Hangfire;
using service.Dto;
using Service.Dto;

namespace service.Job;

public class TaskService : ITaskService
{
    private const string TIMEZONE_ID = "E. South America Standard Time";

    [Obsolete]
    public Result<List<TaskRequest>> CreateTask(List<TaskRequest> tasks)
    {
        try
        { 
            foreach (var task in tasks)
            {
                RecurringJob.AddOrUpdate<HangFireJobService>(
                    task.TaskJobId,
                    (job) => job.SendMessageToQueue(new RequestJob {
                        Message = new RequestJobData { Action = task.Action, Port = task.Port },
                        Queue = task.Queue
                    }),
                    $"{task.Expression}",
                    TimeZoneInfo.FindSystemTimeZoneById(TIMEZONE_ID)
                );
            }

            return Result<List<TaskRequest>>.Create(tasks);
        }
        catch (Exception)
        {
            return Result<List<TaskRequest>>.Fail("Erro ao executar CreateTask");
        }
    }

    public Result<string> OnOffTask(RequestJob request)
    {
        try
        {
            string MESSAGE_RESULT = $"Task {request.Message?.Action.ToUpper()} criada com sucesso.";
            BackgroundJob.Enqueue<HangFireJobService>(job => job.SendMessageToQueue(request));
        
            return Result<string>.Ok(MESSAGE_RESULT);
        }
        catch (Exception )
        {
            return Result<string>.Fail("Erro ao executar OnOffTask");
        }
    }

    public Result<string> DeleteTask(string recurringJobId)
    {
        try
        {
            RecurringJob.RemoveIfExists(recurringJobId);
            return Result<string>.Ok("Task removida com sucesso.");
        }
        catch (Exception)
        {
            return Result<string>.Fail("Falha ao remover task.");
        }
    }
}
