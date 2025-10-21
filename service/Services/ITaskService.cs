using service.Dto;
using Service.Dto;

namespace service.Job;

public interface ITaskService
{
    Result<List<TaskRequest>> CreateTask(List<TaskRequest> tasks, CancellationToken cancellationToken);
    Result<string> OnOffTask(RequestJob request, CancellationToken cancellationToken);
    Result<string> DeleteTask(string recurringJobId);
}