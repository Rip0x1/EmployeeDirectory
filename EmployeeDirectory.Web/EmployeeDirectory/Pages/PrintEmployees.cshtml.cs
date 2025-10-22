using EmployeeDirectory.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EmployeeDirectory.Pages
{
    public class PrintEmployeesModel : PageModel
    {
        private readonly IEmployeeService _employeeService;

        public PrintEmployeesModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public List<EmployeeDirectory.Models.Employee> Employees { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? PhoneSearch { get; set; }
        public string? PositionSearch { get; set; }
        public List<int>? SelectedDepartmentIds { get; set; }
        public List<string>? SelectedDepartmentNames { get; set; }
        public List<string>? SelectedEmployeeNames { get; set; }
        public List<string>? SelectedPositionNames { get; set; }
        public string? SortBy { get; set; }
        public string? DepartmentSearch { get; set; }
        
        public bool HasFilters => !string.IsNullOrEmpty(SearchTerm) || 
                                 !string.IsNullOrEmpty(PhoneSearch) ||
                                 !string.IsNullOrEmpty(PositionSearch) ||
                                 !string.IsNullOrEmpty(DepartmentSearch) ||
                                 (SelectedDepartmentIds?.Any() == true) || 
                                 (SelectedEmployeeNames?.Any() == true) || 
                                 (SelectedPositionNames?.Any() == true) ||
                                 !string.IsNullOrEmpty(SortBy);

        public async Task OnGetAsync(string? search, string? phoneSearch, string? positionSearch, string? departmentSearch, int[]? departmentId, 
            string? employeeNames, string? positionNames, string? sortBy)
        {
            SearchTerm = search;
            PhoneSearch = phoneSearch;
            PositionSearch = positionSearch;
            DepartmentSearch = departmentSearch;
            SortBy = sortBy;
            
            if (departmentId?.Any() == true)
            {
                SelectedDepartmentIds = departmentId.ToList();
            }
            
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
                    e.FullName != null && e.FullName.Contains(SearchTerm));
            }

            if (!string.IsNullOrEmpty(PhoneSearch))
            {
                filteredEmployees = filteredEmployees.Where(e => 
                    (e.CityPhone != null && e.CityPhone.Contains(PhoneSearch)) ||
                    (e.LocalPhone != null && e.LocalPhone.Contains(PhoneSearch)));
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
                    (e.Department != null && e.Department.Name.Contains(DepartmentSearch, StringComparison.OrdinalIgnoreCase)));
            }

            if (SelectedDepartmentIds?.Any() == true)
            {
                filteredEmployees = filteredEmployees.Where(e => 
                    SelectedDepartmentIds.Contains(e.DepartmentId));
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
                    (e.Position != null && e.Position.Name != null && SelectedPositionNames.Contains(e.Position.Name)));
            }


            switch (SortBy)
            {
                case "name":
                    filteredEmployees = filteredEmployees.OrderBy(e => e.FullName);
                    break;
                case "department":
                    filteredEmployees = filteredEmployees.OrderBy(e => e.DepartmentName ?? e.Department!.Name);
                    break;
                case "position":
                    filteredEmployees = filteredEmployees.OrderBy(e => e.PositionDescription ?? e.Position!.Name);
                    break;
                default:
                    filteredEmployees = filteredEmployees.OrderBy(e => e.FullName);
                    break;
            }

            Employees = filteredEmployees.ToList();

            if (SelectedDepartmentIds?.Any() == true)
            {
                var departmentService = HttpContext.RequestServices.GetRequiredService<IDepartmentService>();
                var departments = await departmentService.GetAllDepartmentsAsync();
                SelectedDepartmentNames = departments
                    .Where(d => SelectedDepartmentIds.Contains(d.Id))
                    .Select(d => d.Name)
                    .ToList();
            }
        }
    }
}
