using EmployeeDirectory.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EmployeeDirectory.Pages.Print
{
    public class PrintDepartmentsModel : PageModel
    {
        private readonly IEmployeeService _employeeService;

        public PrintDepartmentsModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public List<Models.Employee> Employees { get; set; } = new();
        public string DepartmentName { get; set; } = string.Empty;
        public string? SearchTerm { get; set; }
        public string? PhoneSearch { get; set; }
        public List<string>? SelectedEmployeeNames { get; set; }
        public List<string>? SelectedPositionNames { get; set; }
        public string? PositionSearch { get; set; }
        public string? EmailSearch { get; set; }
        
        public bool HasFilters => !string.IsNullOrEmpty(SearchTerm) || 
                                 !string.IsNullOrEmpty(PhoneSearch) ||
                                 !string.IsNullOrEmpty(PositionSearch) ||
                                 !string.IsNullOrEmpty(EmailSearch) ||
                                 SelectedEmployeeNames?.Any() == true || 
                                 SelectedPositionNames?.Any() == true;

        public async Task OnGetAsync(int departmentId, string? search, string? phoneSearch, string? positionSearch, string? emailSearch,
            string? employeeNames, string? positionNames)
        {
            SearchTerm = search;
            PhoneSearch = phoneSearch;
            PositionSearch = positionSearch;
            EmailSearch = emailSearch;
            
            if (!string.IsNullOrEmpty(employeeNames))
            {
                SelectedEmployeeNames = employeeNames.Split(',').Select(s => s.Trim()).ToList();
            }
            
            if (!string.IsNullOrEmpty(positionNames))
            {
                SelectedPositionNames = positionNames.Split(',').Select(s => s.Trim()).ToList();
            }

            var allEmployees = await _employeeService.GetEmployeesByDepartmentAsync(departmentId);
            var filteredEmployees = allEmployees.AsQueryable();

            if (allEmployees.Any())
            {
                DepartmentName = allEmployees.First().Department.Name;
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                filteredEmployees = filteredEmployees.Where(e => 
                    e.FullName != null && e.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    e.PositionDescription != null && e.PositionDescription.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    e.Position != null && e.Position.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                );
            }

            if (!string.IsNullOrEmpty(PhoneSearch))
            {
                filteredEmployees = filteredEmployees.Where(e => 
                    e.CityPhone != null && e.CityPhone.Contains(PhoneSearch, StringComparison.OrdinalIgnoreCase) ||
                    e.LocalPhone != null && e.LocalPhone.Contains(PhoneSearch, StringComparison.OrdinalIgnoreCase)
                );
            }

            if (!string.IsNullOrEmpty(PositionSearch))
            {
                filteredEmployees = filteredEmployees.Where(e => 
                    e.PositionDescription != null && e.PositionDescription.Contains(PositionSearch, StringComparison.OrdinalIgnoreCase) ||
                    e.Position != null && e.Position.Name.Contains(PositionSearch, StringComparison.OrdinalIgnoreCase)
                );
            }

            if (!string.IsNullOrEmpty(EmailSearch))
            {
                filteredEmployees = filteredEmployees.Where(e => 
                    !string.IsNullOrEmpty(e.Email) && e.Email.Contains(EmailSearch, StringComparison.OrdinalIgnoreCase)
                );
            }

            if (SelectedEmployeeNames?.Any() == true)
            {
                filteredEmployees = filteredEmployees.Where(e => 
                    e.FullName != null && SelectedEmployeeNames.Contains(e.FullName));
            }

            if (SelectedPositionNames?.Any() == true)
            {
                filteredEmployees = filteredEmployees.Where(e => 
                    !string.IsNullOrEmpty(e.PositionDescription) && SelectedPositionNames.Contains(e.PositionDescription) ||
                    e.Position != null && SelectedPositionNames.Contains(e.Position.Name));
            }


            Employees = filteredEmployees.ToList();
        }
    }
}


