using Refit;
using service.Dto;


namespace service.Services
{
	public interface ISendStatusNotification
	{
        [Post("/notification/statusequipament")]
        Task SendStatusEquipamentAsync([Body] TaskRequest request);

        [Post("/notification/statusmonitoring")]
        Task StatusMonitoringEquipament([Body] List<FreezerMonitoringStatusResponse> request);
    }
}

