
namespace service.Dto
{
    public class EquipamentStateResponse
    {
        public Guid EquipamentId { get; set; }
        public int? State { get; set; }
        public int? OnOff { get; set; }
        public DateTime? LastStateDate { get; set; }
    }
}