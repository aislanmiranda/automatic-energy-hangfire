
using service.Models;

namespace service.Repository;

public interface ITaskRepository
{
    Task<ScheduleTask?> GetByIdAsync(string taskJobId);   
}