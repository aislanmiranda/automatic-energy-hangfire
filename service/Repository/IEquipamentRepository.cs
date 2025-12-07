
using service.Models;

namespace service.Repository;

public interface IEquipamentRepository
{
    Task<Equipament> GetEquipamentByTopic(string topic,
            CancellationToken cancellationToken);

    Task<Equipament> UpdateEquipamentAsync(Equipament entity);
}