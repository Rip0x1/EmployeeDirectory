using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Services;
using EmployeeDirectory.Models;

namespace EmployeeDirectory.Pages.Print;

    public class PrintIndexModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;

    public PrintIndexModel(IEmployeeService employeeService, IDepartmentService departmentService)
        {
            _employeeService = employeeService;
        _departmentService = departmentService;
    }

    public IEnumerable<Employee> Employees { get; set; } = new List<Employee>();
    public IEnumerable<Department> Departments { get; set; } = new List<Department>();

    public async Task OnGetAsync(string? orientation = "portrait", int? scale = 100, string[]? selectedDepartments = null, string? departmentSearch = null)
    {
        var allEmployees = await _employeeService.GetAllEmployeesAsync();
        
        
        if (selectedDepartments != null && selectedDepartments.Length > 0)
        {
            var departmentIds = selectedDepartments
                .Where(id => int.TryParse(id, out _))
                .Select(id => int.Parse(id))
                .ToList();
                
            Employees = allEmployees.Where(e => 
                departmentIds.Contains(e.DepartmentId)
            ).ToList();
        }
        else if (!string.IsNullOrEmpty(departmentSearch))
        {
            Employees = allEmployees.Where(e => 
                (e.DepartmentName ?? e.Department?.Name ?? "").ToLower().Contains(departmentSearch.ToLower())
            ).ToList();
        }
        else
        {
            Employees = allEmployees;
        }
        
        Departments = await _departmentService.GetAllDepartmentsAsync();
        
        ViewData["Orientation"] = orientation ?? "portrait";
        ViewData["Scale"] = scale ?? 100;
    }
}