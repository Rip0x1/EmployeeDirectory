using EmployeeDirectory.Data;
using EmployeeDirectory.Models;
using EmployeeDirectory.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDirectory.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class LogsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LogsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<LogEntry> Items { get; set; } = new();

        public string? Q { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public string? FromDateString => From?.ToString("yyyy-MM-dd");
        public string? ToDateString => To?.ToString("yyyy-MM-dd");

        public async Task OnGetAsync(string? q, DateTime? from, DateTime? to)
        {
            Q = q; From = from; To = to;

            var query = _context.Logs.AsQueryable();

            query = query.Where(l => l.Action != "SYSTEM_INIT" && l.EntityType != "System");

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(l => 
                    (l.Details != null && EF.Functions.ILike(l.Details, $"%{q}%")) ||
                    (l.UserName != null && EF.Functions.ILike(l.UserName, $"%{q}%")) ||
                    (l.Action != null && EF.Functions.ILike(l.Action, $"%{q}%")) ||
                    (l.EntityType != null && EF.Functions.ILike(l.EntityType, $"%{q}%")) ||
                    (l.EntityId != null && EF.Functions.ILike(l.EntityId, $"%{q}%")) ||
                    (l.IpAddress != null && EF.Functions.ILike(l.IpAddress, $"%{q}%"))
                );
            }

            if (from.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(from.Value, DateTimeKind.Utc);
                query = query.Where(l => l.TimestampUtc >= fromUtc);
            }
            if (to.HasValue)
            {
                var toUtc = DateTime.SpecifyKind(to.Value.AddDays(1), DateTimeKind.Utc);
                query = query.Where(l => l.TimestampUtc <= toUtc);
            }

            Items = await query
                .OrderByDescending(l => l.TimestampUtc)
                .Take(500)
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetTableAsync(string? q, DateTime? from, DateTime? to)
        {
            await OnGetAsync(q, from, to);
            
            var html = "";
            foreach (var log in Items)
            {
                html += $@"<tr>
                    <td class=""text-center"" style=""border-right: 3px solid rgb(33, 37, 41) !important;"">{log.TimestampUtc:yyyy-MM-dd HH:mm:ss}</td>
                    <td class=""text-center"" style=""border-right: 3px solid rgb(33, 37, 41) !important;"">{log.UserName}</td>
                    <td class=""text-center"" style=""border-right: 3px solid rgb(33, 37, 41) !important;"">{log.Action}</td>
                    <td class=""text-center"" style=""border-right: 3px solid rgb(33, 37, 41) !important;"">{log.IpAddress ?? "-"}</td>
                    <td class=""text-center"" style=""border-right: 3px solid rgb(33, 37, 41) !important;"">{log.EntityType} ({log.EntityId})</td>
                    <td class=""text-center"" style=""border-right: 3px solid rgb(33, 37, 41) !important;"">{log.Details}</td>
                </tr>";
            }
            
            return Content(html, "text/html");
        }

        public async Task<IActionResult> OnPostClearLogsAsync([FromServices] ILogService logService)
        {
            await logService.DeleteAllLogsAsync();
            TempData["Success"] = "Логи успешно очищены";
            return RedirectToPage("/Admin/Logs");
        }
    }
}


