using Quartz;
using service.Dto;
using service.Models;
using service.Queue;
using service.Repository;
using service.Services;

namespace service.Job;

public class MonitoringQueueJob : IJob
{
    private readonly IRabbitMQService _rabbitMqService;
    private readonly IEquipamentRepository _equipamentRepository;
    private readonly ISendStatusNotification _sendStatusNotification;

    public MonitoringQueueJob(IRabbitMQService rabbitMqService,
        IEquipamentRepository equipamentRepository, ISendStatusNotification sendStatusNotification)
    {
        _rabbitMqService = rabbitMqService;
        _equipamentRepository = equipamentRepository;
        _sendStatusNotification = sendStatusNotification;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var ignores = new[] { "LAST_STATE" };

            // 1. Buscar tópicos ativos no RabbitMQ
            var activeTopics = await _rabbitMqService.ListActiveTopicsAsync(ignores);

            // 2. Buscar equipamentos
            var equipaments = await _equipamentRepository.GetEquipaments(CancellationToken.None);

            // 3. Agora Brasil
            var brazilTz = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            var brazilNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brazilTz);
            var utcTime = DateTime.SpecifyKind(brazilNow, DateTimeKind.Utc);

            // Lista para armazenar somente os equipamentos que mudaram de estado
            var changedEquipaments = new List<Equipament>();

            // 4. Atualizar somente quando houver mudança
            foreach (var equipament in equipaments)
            {
                var topicIsActive = activeTopics.Contains(equipament.Queue);
                var newState = topicIsActive ? 1 : 0;

                if (equipament.State != newState)   // <<< Só atualiza se mudou
                {
                    equipament.UpdateMonitoringEquipament(newState, utcTime);
                    changedEquipaments.Add(equipament);

                    await _equipamentRepository.UpdateEquipamentAsync(equipament);
                }
            }

            if(changedEquipaments.Count > 0)
            {
                // 5. Agrupar APENAS equipamentos com estado alterado
                var grouped = changedEquipaments
                    .GroupBy(e => e.CustomerId)
                    .Select(g => new FreezerMonitoringStatusResponse
                    {
                        CustomerId = g.Key,
                        Equipaments = g.Select(e => new EquipamentStateResponse
                        {
                            EquipamentId = e.Id,
                            State = e.State,
                            OnOff = e.OnOff,
                            LastStateDate = e.LastStateDate
                        }).ToList()
                    })
                    .ToList();

                await _sendStatusNotification.StatusMonitoringEquipament(grouped);
            }

        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }        
    }
}

