using EmployeeDirectory.Models;

namespace EmployeeDirectory.Services
{
    public interface ILogService
    {
        Task WriteAsync(LogEntry entry, CancellationToken cancellationToken = default);
    }
}



