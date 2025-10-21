
using Microsoft.EntityFrameworkCore;
using service.Models;
using service.Repository.Context;

namespace service.Repository;

public class EquipamentRepository : IEquipamentRepository
{
    protected readonly AppDbContext _context;

    public EquipamentRepository(AppDbContext context)
        => _context = context;

    public async Task<Equipament?> GetIdAsync(Guid equipamentId,
            CancellationToken cancellationToken) => await _context
                    .Set<Equipament>()
                    .Where(p => p.Id == equipamentId)
                    .FirstOrDefaultAsync();
}