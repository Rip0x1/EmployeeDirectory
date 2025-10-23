using EmployeeDirectory.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EmployeeDirectory.Pages
{
    public class PrintIndexModel : PageModel
    {
        private readonly IEmployeeService _employeeService;

        public PrintIndexModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public List<EmployeeDirectory.Models.Employee> Employees { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? PhoneSearch { get; set; }
        public int? SelectedDepartmentId { get; set; }
        public string? SelectedDepartmentName { get; set; }
        public List<string>? SelectedEmployeeNames { get; set; }
        public List<string>? SelectedPositionNames { get; set; }
        public string? PositionSearch { get; set; }
        public string? DepartmentSearch { get; set; }
        
        public bool HasFilters => !string.IsNullOrEmpty(SearchTerm) || 
                                 !string.IsNullOrEmpty(PhoneSearch) ||
                                 !string.IsNullOrEmpty(PositionSearch) ||
                                 !string.IsNullOrEmpty(DepartmentSearch) ||
                                 SelectedDepartmentId.HasValue || 
                                 (SelectedEmployeeNames?.Any() == true) || 
                                 (SelectedPositionNames?.Any() == true);

        public async Task OnGetAsync(string? search, string? phoneSearch, string? positionSearch, string? departmentSearch, int? departmentId, string? departmentName, 
            string? employeeNames, string? positionNames)
        {
            SearchTerm = search;
            PhoneSearch = phoneSearch;
            PositionSearch = positionSearch;
            DepartmentSearch = departmentSearch;
            SelectedDepartmentId = departmentId;
            SelectedDepartmentName = departmentName;
            
            if (!string.IsNullOrEmpty(employeeNames))
            {
                SelectedEmployeeNames = employeeNames.Split(',').Select(s => s.Trim()).ToList();
            }
            
            if (!string.IsNullOrEmpty(positionNames))
            {
                SelectedPositionNames = positionNames.Split(',').Select(s => s.Trim()).ToList();
            }

            var allEmployees = await _employeeService.GetAllEmployeesAsync();
            var filteredEmployees = allEmployees.AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                filteredEmployees = filteredEmployees.Where(e => 
                    (e.FullName != null && e.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Department.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (e.PositionDescription != null && e.PositionDescription.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Position != null && e.Position.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrEmpty(PhoneSearch))
            {
                filteredEmployees = filteredEmployees.Where(e => 
                    (e.CityPhone != null && e.CityPhone.Contains(PhoneSearch, StringComparison.OrdinalIgnoreCase)) ||
                    (e.LocalPhone != null && e.LocalPhone.Contains(PhoneSearch, StringComparison.OrdinalIgnoreCase))
                );
            }

            if (!string.IsNullOrEmpty(PositionSearch))
            {
                filteredEmployees = filteredEmployees.Where(e => 
                    (e.PositionDescription != null && e.PositionDescription.Contains(PositionSearch, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Position != null && e.Position.Name.Contains(PositionSearch, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrEmpty(DepartmentSearch))
            {
                filteredEmployees = filteredEmployees.Where(e => 
                    (!string.IsNullOrEmpty(e.DepartmentName) && e.DepartmentName.Contains(DepartmentSearch, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Department != null && e.Department.Name.Contains(DepartmentSearch, StringComparison.OrdinalIgnoreCase))
                );
            }

            if (SelectedDepartmentId.HasValue)
            {
                filteredEmployees = filteredEmployees.Where(e => e.DepartmentId == SelectedDepartmentId.Value);
            }

            if (SelectedEmployeeNames?.Any() == true)
            {
                filteredEmployees = filteredEmployees.Where(e => 
                    e.FullName != null && SelectedEmployeeNames.Contains(e.FullName));
            }

            if (SelectedPositionNames?.Any() == true)
            {
                filteredEmployees = filteredEmployees.Where(e => 
                    (!string.IsNullOrEmpty(e.PositionDescription) && SelectedPositionNames.Contains(e.PositionDescription)) ||
                    (e.Position != null && SelectedPositionNames.Contains(e.Position.Name)));
            }

            Employees = filteredEmployees.ToList();
        }
    }
}

