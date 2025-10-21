
namespace service.Models;

public class ScheduleTask
{
    public int Id { get; protected set; }
    public string? TaskName { get; protected set; }
    public string TaskLegend { get; set; } = string.Empty;

    public string Action { get; protected set; } = string.Empty;
    public string Expression { get; protected set; } = string.Empty;
    public string TaskJobId { get; set; } = string.Empty;

    public Guid EquipamentId { get; protected set; }
    public Equipament? Equipament { get; protected set; }
}
