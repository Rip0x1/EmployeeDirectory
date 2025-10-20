using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Services;
using EmployeeDirectory.Models;

namespace EmployeeDirectory.Pages.Employees
{
    [Authorize(Roles = "Administrator")]
    public class ManageModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IPositionService _positionService;

        public ManageModel(
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            IPositionService positionService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _positionService = positionService;
        }

        public IEnumerable<Employee> Employees { get; set; } = new List<Employee>();
        public IEnumerable<Department> Departments { get; set; } = new List<Department>();
        public IEnumerable<Position> Positions { get; set; } = new List<Position>();

        public async Task OnGetAsync()
        {
            Employees = await _employeeService.GetAllEmployeesAsync();
            Departments = await _departmentService.GetAllDepartmentsAsync();
            Positions = await _positionService.GetAllPositionsAsync();
        }
    }
}

