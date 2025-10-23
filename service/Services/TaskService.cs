using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Quartz;
using service.Dto;
using service.Models;
using service.Queue;
using service.Repository;
using Service.Dto;

namespace service.Job;

public class TaskService : ITaskService
{
    private const string TIMEZONE_ID = "E. South America Standard Time";
    private readonly IScheduler _scheduler;

    public TaskService(IScheduler scheduler)
        => _scheduler = scheduler;

    //[Obsolete]
    //public Result<List<TaskRequest>> CreateTask(List<TaskRequest> tasks, CancellationToken cancellationToken)
    //{
    //    try
    //    { 
    //        foreach (var task in tasks)
    //        {
    //            RecurringJob.AddOrUpdate<HangFireJobService>(
    //                task.TaskJobId,
    //                (job) => job.SendMessageToQueue(new RequestJob {
    //                    Message = new RequestJobData
    //                    {
    //                        Action = task.Action,
    //                        Port = task.Port
    //                    },
    //                    Queue = task.Queue
    //                }, cancellationToken),
    //                $"{task.Expression}",
    //                TimeZoneInfo.FindSystemTimeZoneById(TIMEZONE_ID)
    //            );
    //        }

    //        return Result<List<TaskRequest>>.Create(tasks);
    //    }
    //    catch (Exception)
    //    {
    //        return Result<List<TaskRequest>>.Fail("Erro ao executar CreateTask");
    //    }
    //}

    public async Task<Result<List<TaskRequest>>> CreateTaskNewAsync(List<TaskRequest> tasks, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var task in tasks)
            {
                var jobKey = new JobKey(task.TaskJobId);

                await _scheduler.DeleteJob(jobKey);

                var job = JobBuilder.Create<SendMessageJob>()
                    .WithIdentity(jobKey)
                    .UsingJobData(new JobDataMap
                    {
                        ["Request"] = task,
                        ["CancellationToken"] = cancellationToken
                    })
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity($"{task.TaskJobId}-trigger")
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
            var job = JobBuilder.Create<SendMessageJob>()
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
            //RecurringJob.RemoveIfExists(recurringJobId);
            var jobKey = new JobKey(recurringJobId, "default");

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

public class SendMessageJob : IJob
{
    private readonly IRabbitMQService _rabbitMqService;
    private readonly IEquipamentRepository _equipamentRepository;
    private readonly IMonitoringRepository _monitoringRepository;

    public SendMessageJob(IScheduler scheduler, IRabbitMQService rabbitMqService,
        IEquipamentRepository equipamentRepository, IMonitoringRepository monitoringRepository)
    {
        _rabbitMqService = rabbitMqService;
        _equipamentRepository = equipamentRepository;
        _monitoringRepository = monitoringRepository;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {        
        var data = context.MergedJobDataMap;

        var request = data["Request"] as TaskRequest;
        var cancellationToken = (CancellationToken)data["CancellationToken"];

        await _rabbitMqService.PublishMessageAsync(new RequestJob
        {
            EquipamentId = request!.EquipamentId,
            Message = new RequestJobData
            {
                Action = request!.Action,
                Port = request!.Port
            },
            Queue = request!.Queue
        });
                
        var equip = await _equipamentRepository.GetIdAsync(request.EquipamentId, cancellationToken);

        await _monitoringRepository.InsertAsync(new Monitoring
        (
            equipamentId: equip!.Id,
            customerId: equip!.CustomerId,
            action: request!.Action

        ), cancellationToken);
    }
}