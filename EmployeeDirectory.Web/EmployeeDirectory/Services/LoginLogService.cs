using EmployeeDirectory.Data;
using EmployeeDirectory.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDirectory.Services
{
    public class LoginLogService : ILoginLogService
    {
        private readonly ApplicationDbContext _context;

        public LoginLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task WriteLoginLogAsync(LoginLog log, CancellationToken cancellationToken = default)
        {
            log.TimestampUtc = DateTime.UtcNow;
            _context.LoginLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<LoginLog>> GetAllLoginLogsAsync()
        {
            return await _context.LoginLogs
                .OrderByDescending(l => l.TimestampUtc)
                .ToListAsync();
        }

        public async Task<IEnumerable<LoginLog>> GetLoginLogsByDateRangeAsync(DateTime? from, DateTime? to)
        {
            var query = _context.LoginLogs.AsQueryable();

            if (from.HasValue)
            {
                var fromUtc = from.Value.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(from.Value, DateTimeKind.Utc) 
                    : from.Value.ToUniversalTime();
                query = query.Where(l => l.TimestampUtc >= fromUtc);
            }

            if (to.HasValue)
            {
                var toUtc = to.Value.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(to.Value.AddDays(1), DateTimeKind.Utc) 
                    : to.Value.AddDays(1).ToUniversalTime();
                query = query.Where(l => l.TimestampUtc < toUtc);
            }

            return await query
                .OrderByDescending(l => l.TimestampUtc)
                .ToListAsync();
        }

        public async Task DeleteAllLoginLogsAsync()
        {
            var allLogs = await _context.LoginLogs.ToListAsync();
            _context.LoginLogs.RemoveRange(allLogs);
            await _context.SaveChangesAsync();
        }
    }
}

