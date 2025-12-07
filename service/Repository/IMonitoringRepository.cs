
using service.Models;

namespace service.Repository;

public interface IMonitoringRepository
{
    Task<Monitoring> InsertAsync(Monitoring monitor);
}