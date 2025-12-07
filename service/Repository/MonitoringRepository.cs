using Microsoft.EntityFrameworkCore;
using service.Models;
using service.Repository.Context;

namespace service.Repository;

public class MonitoringRepository : IMonitoringRepository
{
    protected readonly AppDbContext _context;

    public MonitoringRepository(AppDbContext context)
        => _context = context;

    public async Task<Monitoring> InsertAsync(Monitoring monitor)
    {
        _context.Set<Monitoring>().Add(monitor);

        var save = await _context.SaveChangesAsync();

        var existingEntity = await _context.Set<Monitoring>()
            .FirstOrDefaultAsync(e => e.Id == monitor.Id);

        if (existingEntity == null)
            throw new KeyNotFoundException("Entity not found.");

        return existingEntity;
    }
}