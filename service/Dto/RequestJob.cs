namespace service.Dto
{
	public class RequestJob
	{
		public RequestJobData? Message { get; set; }
		public string Queue { get; set; } = string.Empty;
    }

    public class RequestJobData
    {
        public string Action { get; set; } = string.Empty;
        public int Port { get; set; }
    }
}

