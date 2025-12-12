using service.Dto;
using Service.Dto;

namespace service.Job;

public interface ITaskService
{
    Task<Result<List<TaskRequest>>> CreateTaskNewAsync(List<TaskRequest> tasks, CancellationToken cancellationToken);
    Task<Result<string>> OnOffTaskAsync(TaskRequest request, CancellationToken cancellationToken);
    Task<Result<string>> ScheduleStateMonitorTaskAsync(int timeMinute, CancellationToken cancellationToken);
    Task<Result<string>> DeleteTaskAsync(string recurringJobId);   
}