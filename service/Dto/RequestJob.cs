namespace service.Dto
{
	public class RequestJob
	{
        public Guid CustomerId { get; set; }
        public Guid EquipamentId { get; set; }
        public string Queue { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool RegisterMonitoring { get; set; }
    }
}

