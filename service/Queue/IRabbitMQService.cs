
using service.Dto;

namespace service.Queue;

public interface IRabbitMQService
{
   Task PublishMessageAsync(RequestJob request);
}