using EmployeeDirectory.Data;
using EmployeeDirectory.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDirectory.Services
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default)
        {
            entry.TimestampUtc = DateTime.UtcNow;
            var http = _httpContextAccessor.HttpContext;
            if (http != null)
            {
                entry.UserId ??= http.User?.FindFirst("sub")?.Value
                                ?? http.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                entry.UserName ??= http.User?.Identity?.Name;
                entry.IpAddress ??= http.Connection?.RemoteIpAddress?.ToString();
            }
            _context.Logs.Add(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<LogEntry>> GetAllLogsAsync()
        {
            return await _context.Logs
                .OrderByDescending(l => l.TimestampUtc)
                .ToListAsync();
        }
    }
}


