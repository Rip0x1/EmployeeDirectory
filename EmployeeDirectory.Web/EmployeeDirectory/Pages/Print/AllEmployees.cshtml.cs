using EmployeeDirectory.Data;
using EmployeeDirectory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeDirectory.Pages.Print
{
    [Authorize]
    public class AllEmployeesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AllEmployeesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<IGrouping<string, Employee>> DepartmentGroups { get; set; } = null!;

        public async Task OnGetAsync()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .ToListAsync();

            DepartmentGroups = employees
                .GroupBy(e => e.Department?.Name ?? "Без отдела")
                .OrderBy(g => g.Key)
                .AsQueryable();
        }
    }
}




