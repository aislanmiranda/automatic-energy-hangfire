using System.Text;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Formatter;
using service.Dto;
using MQTTnet.Protocol;
using System.Threading.Channels;
using service.Repository;
using service.Models;
using System.Net.Http.Headers;

namespace service.Queue;

public class RabbitMQService : IRabbitMQService, IHostedService, IDisposable
{
    private readonly ILogger<RabbitMQService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private IMqttClient? _client;
    private MqttClientOptions? _options;
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    private readonly string _host = Environment.GetEnvironmentVariable("RBHOST") ?? "localhost";
    private readonly int _port = 1883;
    private readonly string _user = Environment.GetEnvironmentVariable("RBUSER") ?? "admin";
    private readonly string _pass = Environment.GetEnvironmentVariable("RBPASS") ?? "public";

    private readonly IList<string> _topicsToListen = new List<string>
    {
        "LAST_STATE_2"
    };

    private readonly Channel<MqttApplicationMessageReceivedEventArgs> _messageQueue =
        Channel.CreateUnbounded<MqttApplicationMessageReceivedEventArgs>();

    public RabbitMQService(IServiceScopeFactory scopeFactory,
        ILogger<RabbitMQService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // =============================================================================================
    // Start / Stop
    // =============================================================================================
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting MQTT Service...");

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        BuildOptions();
        CreateClient();

        _loopTask = Task.Run(() => ConnectionLoopAsync(_cts.Token));
        _ = Task.Run(() => MessageProcessingLoop(_cts.Token));

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping MQTT Service...");

        _cts?.Cancel();

        if (_client?.IsConnected == true)
        {
            try { await _client.DisconnectAsync(); }
            catch { }
        }
    }

    public void Dispose() => _cts?.Dispose();

    // =============================================================================================
    // MQTT Setup
    // =============================================================================================
    private void BuildOptions()
    {
        _options = new MqttClientOptionsBuilder()
            .WithClientId($"backend-{Guid.NewGuid()}")
            .WithTcpServer(_host, _port)
            .WithProtocolVersion(MqttProtocolVersion.V311)  // RabbitMQ exige 3.1.1
            .WithCredentials(_user, _pass)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
            .WithCleanSession()
            .Build();
    }

    private void CreateClient()
    {
        _client = new MqttClientFactory().CreateMqttClient();

        _client.ConnectedAsync += async e =>
        {
            _logger.LogInformation("MQTT CONNECTED (session={Session})",
                e.ConnectResult?.IsSessionPresent);

            var topicFilters = _topicsToListen
             .Select(t => new MqttTopicFilterBuilder()
                 .WithTopic(t)
                 .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                 .Build())
             .ToList();

            await SubscribeTopicsAsync(_client, _topicsToListen);

            _logger.LogInformation("Subscribed to: {Topics}", string.Join(", ", _topicsToListen));

            await Task.CompletedTask;
        };

        _client.ApplicationMessageReceivedAsync += async e =>
        {
            try
            {
                await _messageQueue.Writer.WriteAsync(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao processar mensagem MQTT: {ex.Message}");
            }
        };
    }

    // =============================================================================================
    // Auto-Reconnect Loop
    // =============================================================================================
    private async Task ConnectionLoopAsync(CancellationToken token)
    {
        int delay = 2;

        while (!token.IsCancellationRequested)
        {
            try
            {
                if (!_client!.IsConnected)
                {
                    _logger.LogInformation("Connecting to MQTT {Host}:{Port}...", _host, _port);
                    await _client.ConnectAsync(_options!, token);
                }

                delay = 2; // reset
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MQTT connection failed. Retry in {Delay}s", delay);
                await Task.Delay(TimeSpan.FromSeconds(delay), token);
                delay = Math.Min(delay * 2, 30);
            }

            await Task.Delay(1000, token);
        }
    }

    private async Task MessageProcessingLoop(CancellationToken cancellationToken)
    {
        await foreach (var e in _messageQueue.Reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                await HandleBusinessMessageAsync(e, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem MQTT");
            }
        }
    }

    private async Task HandleBusinessMessageAsync(MqttApplicationMessageReceivedEventArgs e,
        CancellationToken cancellationToken)
    {
        var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
        var topic = e.ApplicationMessage.Topic;

        _logger.LogInformation("Mensagem recebida: {Topic} -> {Payload}", topic, payload);

        var data = JsonSerializer.Deserialize<LastStateResponse>(payload, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // pega estado do equipamento
        var equip = await GetEquipamentByTopic(data!.Topic, cancellationToken);

        var action = equip!.OnOff == 1 ? "ON":"OFF";

        // notifica esp32 o estado atual
        await PublishMessageAsync(new RequestJob
        {
            Queue = data!.Topic,
            Action = action,
            Port = equip.Port
        });
    }

    private async Task<Equipament?> GetEquipamentByTopic(string topic, CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();

            var _equipamentRepository = scope.ServiceProvider.GetRequiredService<IEquipamentRepository>();
            var equipament = await _equipamentRepository
                .GetEquipamentByTopic(topic, stoppingToken);
            Console.WriteLine($"[GetEquipament]: ✅ Equipamento recuperado com sucesso");
            return equipament;
        }
        catch (Exception)
        {
            Console.WriteLine($"[GetEquipament]: ❌ Erro ao recuperar equipamento ");
            return null;
        }
    }

    private async Task SubscribeTopicsAsync(IMqttClient client, IList<string> topics)
    {
        try
        {
            var optionsBuilder = new MqttClientSubscribeOptionsBuilder();

            foreach (var topic in topics)
                optionsBuilder.WithTopicFilter(f => f.WithTopic(topic));

            await client.SubscribeAsync(optionsBuilder.Build());

            Console.WriteLine($"[SubscribeTopicsAsync]: ✅ Inscrito nos tópicos: {string.Join(", ", topics)}");
        }
        catch (Exception)
        {
            Console.WriteLine($"❌ Erro ao executar inscrição nos tópicos: {string.Join(", ", topics)}");
        }
    }

    // =============================================================================================
    // Publish API
    // =============================================================================================
    public async Task PublishMessageAsync(RequestJob request)
    {
        if (_client?.IsConnected != true)
        {
            throw new InvalidOperationException("MQTT is not connected.");
        }

        var json = JsonSerializer.Serialize(request);

        var msg = new MqttApplicationMessageBuilder()
            .WithTopic(request.Queue)
            .WithPayload(Encoding.UTF8.GetBytes(json))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _client.PublishAsync(msg);

        _logger.LogInformation("Published to {Topic}: {Payload}",
            request.Queue, json);
    }

    public async Task<List<string>> ListActiveTopicsAsync()
    {
        using var client = new HttpClient();

        var username = _user;
        var password = _pass;

        var basicAuth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", basicAuth);

        var url = $"http://{_host}:15672/api/exchanges/%2F/amq.topic/bindings/source";

        var json = await client.GetStringAsync(url);

        var items = JsonSerializer.Deserialize<List<RabbitBindingResponse>>(json)!;

        return items
            .Where(b => !string.IsNullOrWhiteSpace(b.routing_key))
            .Select(b => b.routing_key)
            .Distinct()
            .ToList();
    }

    public async Task<List<string>> ListActiveTopicsAsync(IEnumerable<string>? ignoreList = null)
    {
        ignoreList ??= Enumerable.Empty<string>();

        using var client = new HttpClient();

        var basicAuth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_user}:{_pass}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);

        var url = $"http://{_host}:15672/api/exchanges/%2F/amq.topic/bindings/source";
        var json = await client.GetStringAsync(url);

        var items = JsonSerializer.Deserialize<List<RabbitBindingResponse>>(json)!;

        return items
            .Where(b => !string.IsNullOrWhiteSpace(b.routing_key))
            .Select(b => b.routing_key)
            .Where(topic => !ignoreList.Any(prefix => topic.StartsWith(prefix)))
            .Distinct()
            .ToList();
    }

}