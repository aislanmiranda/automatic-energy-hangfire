
namespace service.Models;

public class Equipament
{
    public Guid Id { get; protected set; }
    public string Tag { get; protected set; } = string.Empty;
    public string Queue { get; protected set; } = string.Empty;
    public int Port { get; protected set; }

    public Guid CustomerId { get; protected set; }

    public int OnOff { get; protected set; }
    public int Active { get; protected set; }

    public virtual List<ScheduleTask>? Tasks { get; protected set; }
    public virtual List<Monitoring>? Monitorings { get; set; }

    public void UpdateStateEquipament(int action)
    {
        this.OnOff = action;
    }
}