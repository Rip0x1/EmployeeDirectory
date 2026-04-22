using EmployeeDirectory.Models;

namespace EmployeeDirectory.Services
{
    public interface ILoginLogService
    {
        Task WriteLoginLogAsync(LoginLog log, CancellationToken cancellationToken = default);
        Task<IEnumerable<LoginLog>> GetAllLoginLogsAsync();
        Task<IEnumerable<LoginLog>> GetLoginLogsByDateRangeAsync(DateTime? from, DateTime? to);
        Task DeleteAllLoginLogsAsync();
    }
}

