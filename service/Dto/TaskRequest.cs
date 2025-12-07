namespace service.Dto;

public class TaskRequest
{
	public string Tag { get; set; } = string.Empty;
	public string Queue { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Expression { get; set; } = string.Empty;
    public string TaskJobId { get; set; } = string.Empty;
    public Guid EquipamentId { get; set; }
    public Guid CustomerId { get; set; }
    public bool RegisterMonitoring { get; set; }   
}