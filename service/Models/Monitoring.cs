
namespace service.Models;

public class Monitoring
{
    public Monitoring(Guid equipamentId, Guid customerId, string action)
    {
        EquipamentId = equipamentId;
        CustomerId = customerId;
        Action = action;
    }

    public Guid Id { get; protected set; }
    public Guid CustomerId { get; protected set; }
    public Guid EquipamentId { get; protected set; }
    public string Action { get; protected set; } = string.Empty;
    public DateTime DateAction { get; protected set; } 
}