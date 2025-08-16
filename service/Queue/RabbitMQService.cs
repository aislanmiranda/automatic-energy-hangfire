using System.Text;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using service.Dto;

namespace service.Queue;

public class RabbitMQService : IRabbitMQService
{
    private static readonly string Broker = Environment.GetEnvironmentVariable("RBHOST") ?? string.Empty;
    private static readonly int Port = 1883;
    private static readonly string User = Environment.GetEnvironmentVariable("RBUSER") ?? string.Empty;
    private static readonly string Password = Environment.GetEnvironmentVariable("RBPASS") ?? string.Empty;

    public async Task PublishMessageAsync(RequestJob request)
    {
        var factory = new MqttFactory();
        var client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(Broker, Port)
            .WithCredentials(User, Password)
            .WithCleanSession()
            .Build();

        await client.ConnectAsync(options, CancellationToken.None);
        string messageJson = JsonSerializer.Serialize(request.Message);

        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(request.Queue)
            .WithPayload(Encoding.UTF8.GetBytes(messageJson))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await client.PublishAsync(mqttMessage, CancellationToken.None);

        Console.WriteLine($"[MQTT] Sent: {request}");

        await client.DisconnectAsync();
    }
}