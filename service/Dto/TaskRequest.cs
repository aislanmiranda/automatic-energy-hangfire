namespace service.Dto;

public class TaskRequest
{
	public string Queue { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Expression { get; set; } = string.Empty;
    public string TaskJobId { get; set; } = string.Empty;
}