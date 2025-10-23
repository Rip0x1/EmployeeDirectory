using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Services;
using EmployeeDirectory.Models;
using Microsoft.EntityFrameworkCore;
using EmployeeDirectory.Data;

namespace EmployeeDirectory.Pages.Print;

public class PrintLogsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public PrintLogsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IEnumerable<LogEntry> Logs { get; set; } = new List<LogEntry>();

    public async Task OnGetAsync(string? orientation = "portrait", int? scale = 100, string? q = null, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Logs.AsQueryable();

        query = query.Where(l => l.Action != "SYSTEM_INIT" && l.EntityType != "System");

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(l => 
                (l.Details != null && EF.Functions.ILike(l.Details, $"%{q}%")) ||
                (l.UserName != null && EF.Functions.ILike(l.UserName, $"%{q}%")) ||
                (l.Action != null && EF.Functions.ILike(l.Action, $"%{q}%")) ||
                (l.EntityType != null && EF.Functions.ILike(l.EntityType, $"%{q}%")) ||
                (l.EntityId != null && EF.Functions.ILike(l.EntityId, $"%{q}%"))
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

        Logs = await query
            .OrderByDescending(l => l.TimestampUtc)
            .ToListAsync();
        
        ViewData["Orientation"] = orientation ?? "portrait";
        ViewData["Scale"] = scale ?? 100;
    }
}