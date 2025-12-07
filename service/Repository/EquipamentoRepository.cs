using System;
using Microsoft.EntityFrameworkCore;
using service.Models;
using service.Repository.Context;

namespace service.Repository
{
    public class EquipamentRepository : IEquipamentRepository
    {
        protected readonly AppDbContext _context;

        public EquipamentRepository(AppDbContext context)
            => _context = context;

        public async Task<Equipament> GetEquipamentByTopic(string topic,
            CancellationToken cancellationToken)
        {
            var equipament = await _context
                                .Set<Equipament>()
                                .Where(p => p.Queue == topic)
                                .FirstOrDefaultAsync(cancellationToken);
            return equipament!;
        }

        public async Task<Equipament> UpdateEquipamentAsync(Equipament entity)
        {
            var model = _context.Update(entity).Entity;
            await _context.SaveChangesAsync();
            return model;
        }
    }
}

