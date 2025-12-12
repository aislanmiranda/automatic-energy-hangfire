using Quartz;
using service.Dto;
using service.Models;
using service.Queue;
using service.Repository;
using service.Services;

namespace service.Job;

public class SendMessageQueueJob : IJob
{
    private readonly IRabbitMQService _rabbitMqService;
    private readonly IMonitoringRepository _monitoringRepository;
    private readonly IEquipamentRepository _equipamentRepository;
    private readonly ISendStatusNotification _sendStatusNotification;

    public SendMessageQueueJob(IRabbitMQService rabbitMqService, IMonitoringRepository monitoringRepository,
        IEquipamentRepository equipamentRepository, ISendStatusNotification sendStatusNotification)
    {
        _rabbitMqService = rabbitMqService;
        _monitoringRepository = monitoringRepository;
        _equipamentRepository = equipamentRepository;
        _sendStatusNotification = sendStatusNotification;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var data = context.MergedJobDataMap;

            var request = data["Request"] as TaskRequest;
            var notification = new RequestJob
            {
                CustomerId = request!.CustomerId,
                EquipamentId = request!.EquipamentId,
                Action = request!.Action,
                Port = request!.Port,
                Queue = request!.Queue,
                RegisterMonitoring = request!.RegisterMonitoring
            };

            await _rabbitMqService.PublishMessageAsync(notification);

            await _monitoringRepository.InsertAsync(new Monitoring(
                equipamentId: request!.EquipamentId,
                customerId: request!.CustomerId,
                action: request!.Action));

            //atualiza estado equipamento
            var equipament = await _equipamentRepository.GetEquipamentByTopic(notification.Queue, CancellationToken.None);
            equipament.UpdateStateEquipament(notification.Action == "ON" ? 1:0);
            await _equipamentRepository.UpdateEquipamentAsync(equipament);

            //notifica o manager a mudança de estado
            await _sendStatusNotification.SendStatusEquipamentAsync(request);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}

