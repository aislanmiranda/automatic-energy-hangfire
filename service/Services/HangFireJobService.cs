using service.Dto;
using service.Queue;
using service.Repository;

namespace service.Job;

//public class HangFireJobService
//{
//    private readonly IRabbitMQService _rabbitMqService;
//    private readonly ITaskRepository _taskRepository;

//    public HangFireJobService(IRabbitMQService rabbitMqService, ITaskRepository taskRepository)
//    {
//        _rabbitMqService = rabbitMqService;
//        _taskRepository = taskRepository;
//    }
    
//    public async Task SendMessageToQueue(RequestJob request, CancellationToken cancellationToken)
//    {

//        try
//        {
//            //        private readonly IEquipamentRepository _equipamentRepository;
//            //private readonly IMonitoringRepository _monitoringRepository;
//            //var equip = await _equipamentRepository.GetIdAsync(request.Message!.EquipamentId, cancellationToken);

//            //await _monitoringRepository.InsertAsync(new Monitoring
//            //(
//            //    equipamentId: equip!.Id,
//            //    customerId: equip!.CustomerId,
//            //    action: request.Message.Action
//            //), cancellationToken);

//            await _rabbitMqService.PublishMessageAsync(request);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine(ex);
//        }
//    }

//    public async Task ExecuteTaskAsync(string taskJobId)
//    {
//        try
//        {
//            var task = await _taskRepository.GetByIdAsync(taskJobId);

//            if (task == null)
//                return;

//            var request = new RequestJob
//            {
//                Message = new RequestJobData
//                {
//                    Action = task.Action,
//                    Port = task.Equipament!.Port
//                },
//                Queue = task.Equipament.Queue
//            };

//            await _rabbitMqService.PublishMessageAsync(request);
//        }
//        catch (Exception ex)
//        {
//            throw new Exception(ex.Message);
//        }
//    }
//}