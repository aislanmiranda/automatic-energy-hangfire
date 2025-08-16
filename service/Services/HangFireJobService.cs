
using service.Dto;
using service.Queue;

namespace service.Job;

public class HangFireJobService
{
    private readonly IRabbitMQService _rabbitMqService;

    public HangFireJobService(IRabbitMQService rabbitMqService)
        => _rabbitMqService = rabbitMqService;
    
    public async Task SendMessageToQueue(RequestJob request)
    {
        await _rabbitMqService.PublishMessageAsync(request);
        Console.WriteLine($"Message sent: {request.Message}");
    }
}