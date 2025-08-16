using service.Dto;
using Service.Dto;

namespace service.Job;

public interface ITaskService
{
    Result<List<TaskRequest>> CreateTask(List<TaskRequest> tasks);
    Result<string> OnOffTask(RequestJob request);
    Result<string> DeleteTask(string recurringJobId);
}