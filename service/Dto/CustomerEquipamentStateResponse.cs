
namespace service.Dto
{
    public class FreezerMonitoringStatusResponse
    {
        public Guid CustomerId { get; set; }
        public List<EquipamentStateResponse> Equipaments { get; set; } = new();
    }
}