using EmployeeDirectory.Data;
using EmployeeDirectory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace EmployeeDirectory.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class PrintLogsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PrintLogsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<LogEntry> Items { get; set; } = new();

        public string? Q { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public async Task OnGetAsync(string? q, DateTime? from, DateTime? to)
        {
            Q = q; From = from; To = to;

            var query = _context.Logs.AsQueryable();

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

            Items = await query
                .OrderByDescending(l => l.TimestampUtc)
                .ToListAsync();
        }
    }
}




