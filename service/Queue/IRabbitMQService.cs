
using service.Dto;

namespace service.Queue;

public interface IRabbitMQService: IHostedService
{
    Task PublishMessageAsync(RequestJob request);
    Task<List<string>> ListActiveTopicsAsync();
}