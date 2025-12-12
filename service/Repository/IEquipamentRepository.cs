
using service.Models;

namespace service.Repository;

public interface IEquipamentRepository
{
    Task<List<Equipament>> GetEquipaments(CancellationToken cancellationToken);

    Task<Equipament> GetEquipamentByTopic(string topic,
            CancellationToken cancellationToken);

    Task<Equipament> UpdateEquipamentAsync(Equipament entity);
}