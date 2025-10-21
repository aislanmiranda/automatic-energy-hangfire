
using service.Models;

namespace service.Repository;

public interface IEquipamentRepository
{
    Task<Equipament?> GetIdAsync(Guid equipament,
        CancellationToken cancellationToken);
}