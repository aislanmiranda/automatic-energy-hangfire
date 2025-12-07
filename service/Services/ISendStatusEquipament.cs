using Refit;
using service.Dto;


namespace service.Services
{
	public interface ISendStatusEquipament
	{
        [Post("/equipament/status")]
        Task SendStatusEquipamentAsync([Body] TaskRequest request);
    }
}

