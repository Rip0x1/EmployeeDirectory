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
    public class DepartmentModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DepartmentModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Employee> Employees { get; set; } = new();
        public string DepartmentName { get; set; } = string.Empty;

        public async Task OnGetAsync(int departmentId)
        {
            var department = await _context.Departments.FindAsync(departmentId);
            if (department != null)
            {
                DepartmentName = department.Name;
                
                Employees = await _context.Employees
                    .Where(e => e.DepartmentId == departmentId)
                    .Include(e => e.Department)
                    .ToListAsync();
            }
        }
    }
}




