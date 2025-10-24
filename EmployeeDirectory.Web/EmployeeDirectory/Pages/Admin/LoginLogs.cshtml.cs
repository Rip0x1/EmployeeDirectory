using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Services;
using EmployeeDirectory.Models;

namespace EmployeeDirectory.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class LoginLogsModel : PageModel
    {
        private readonly ILoginLogService _loginLogService;

        public LoginLogsModel(ILoginLogService loginLogService)
        {
            _loginLogService = loginLogService;
        }

        public IEnumerable<LoginLog> LoginLogs { get; set; } = new List<LoginLog>();

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? From { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? To { get; set; }

        public string FromDateString => From?.ToString("yyyy-MM-dd") ?? "";
        public string ToDateString => To?.ToString("yyyy-MM-dd") ?? "";

        public async Task OnGetAsync()
        {
            if (From.HasValue || To.HasValue)
            {
                LoginLogs = await _loginLogService.GetLoginLogsByDateRangeAsync(From, To);
            }
            else
            {
                LoginLogs = await _loginLogService.GetAllLoginLogsAsync();
            }

            if (!string.IsNullOrEmpty(Q))
            {
                var query = Q.ToLower();
                LoginLogs = LoginLogs.Where(l =>
                    (l.UserName != null && l.UserName.ToLower().Contains(query)) ||
                    (l.IpAddress != null && l.IpAddress.ToLower().Contains(query)) ||
                    (l.Action != null && l.Action.ToLower().Contains(query)) ||
                    (l.FailureReason != null && l.FailureReason.ToLower().Contains(query))
                );
            }
        }

        public async Task<IActionResult> OnGetTableAsync()
        {
            if (From.HasValue || To.HasValue)
            {
                LoginLogs = await _loginLogService.GetLoginLogsByDateRangeAsync(From, To);
            }
            else
            {
                LoginLogs = await _loginLogService.GetAllLoginLogsAsync();
            }

            if (!string.IsNullOrEmpty(Q))
            {
                var query = Q.ToLower();
                LoginLogs = LoginLogs.Where(l =>
                    (l.UserName != null && l.UserName.ToLower().Contains(query)) ||
                    (l.IpAddress != null && l.IpAddress.ToLower().Contains(query)) ||
                    (l.Action != null && l.Action.ToLower().Contains(query)) ||
                    (l.FailureReason != null && l.FailureReason.ToLower().Contains(query))
                );
            }

            return Partial("_LoginLogsTable", LoginLogs);
        }

        public async Task<IActionResult> OnPostClearLogsAsync()
        {
            await _loginLogService.DeleteAllLoginLogsAsync();
            TempData["Success"] = "Логи успешно очищены";
            return RedirectToPage("/Admin/LoginLogs");
        }
    }
}

