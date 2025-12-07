using Microsoft.EntityFrameworkCore;
using service.Models;
using service.Repository.Context;

namespace service.Repository;

public class TaskRepository : ITaskRepository
{
    protected readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
        => _context = context;

    public async Task<ScheduleTask?> GetByIdAsync(string taskJobId)
        => await _context
            .Set<ScheduleTask>()
            .Include(p => p.Equipament)
            .FirstOrDefaultAsync(p => p.TaskJobId == taskJobId);
}