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
    [AllowAnonymous]
    public class FilteredEmployeesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public FilteredEmployeesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<IGrouping<string, Employee>> DepartmentGroups { get; set; } = null!;
        public string? SearchQuery { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public bool? IsHeadOnly { get; set; }

        public async Task OnGetAsync(string? search, int? departmentId, string? positionSearch, string? phoneSearch, bool? isHeadOnly, string? sortBy)
        {
            SearchQuery = search;
            DepartmentId = departmentId;
            IsHeadOnly = isHeadOnly;

            var employees = _context.Employees
                .Include(e => e.Department)
                .AsQueryable();

            // Применяем те же фильтры, что и на главной странице
            if (!string.IsNullOrEmpty(search))
            {
                employees = employees.Where(e => 
                    (e.FullName != null && EF.Functions.ILike(e.FullName, $"%{search}%")) ||
                    (e.CityPhone != null && EF.Functions.ILike(e.CityPhone, $"%{search}%")) ||
                    (e.LocalPhone != null && EF.Functions.ILike(e.LocalPhone, $"%{search}%"))
                );
            }

            if (departmentId.HasValue)
            {
                employees = employees.Where(e => e.DepartmentId == departmentId.Value);
                var department = await _context.Departments.FindAsync(departmentId.Value);
                DepartmentName = department?.Name;
            }

            if (!string.IsNullOrEmpty(positionSearch))
            {
                employees = employees.Where(e => 
                    e.PositionDescription != null && 
                    EF.Functions.ILike(e.PositionDescription, $"%{positionSearch}%")
                );
            }

            if (!string.IsNullOrEmpty(phoneSearch))
            {
                employees = employees.Where(e => 
                    (e.CityPhone != null && EF.Functions.ILike(e.CityPhone, $"%{phoneSearch}%")) ||
                    (e.LocalPhone != null && EF.Functions.ILike(e.LocalPhone, $"%{phoneSearch}%"))
                );
            }

            if (isHeadOnly.HasValue)
            {
                employees = employees.Where(e => e.IsHeadOfDepartment == isHeadOnly.Value);
            }

            // Сортировка
            var orderedEmployees = employees.OrderBy(e => e.Department != null ? e.Department.Name : "Без отдела")
                                          .ThenBy(e => e.IsHeadOfDepartment ? 0 : 1);

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "name":
                        orderedEmployees = orderedEmployees.ThenBy(e => e.FullName);
                        break;
                    case "position":
                        orderedEmployees = orderedEmployees.ThenBy(e => e.PositionDescription);
                        break;
                    default:
                        orderedEmployees = orderedEmployees.ThenBy(e => e.FullName);
                        break;
                }
            }
            else
            {
                orderedEmployees = orderedEmployees.ThenBy(e => e.FullName);
            }

            var employeeList = await orderedEmployees.ToListAsync();

            DepartmentGroups = employeeList
                .GroupBy(e => e.Department != null ? e.Department.Name : "Без отдела")
                .OrderBy(g => g.Key)
                .AsQueryable();
        }
    }
}
