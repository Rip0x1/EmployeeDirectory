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
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 3;
        public int TotalPages { get; set; }
        public int TotalEmployees { get; set; }
        public int? DepartmentsPerPage { get; set; }

        public async Task OnGetAsync(int pageNumber = 1, int? departmentsPerPage = null)
        {
            PageNumber = pageNumber;
            DepartmentsPerPage = departmentsPerPage;

            var allEmployees = await _employeeService.GetAllEmployeesAsync();
            Departments = await _departmentService.GetAllDepartmentsAsync();
            Positions = await _positionService.GetAllPositionsAsync();

            TotalEmployees = allEmployees.Count();
            
            var groupedEmployees = allEmployees.GroupBy(e => e.Department.Name).OrderBy(g => g.Key).ToList();
            var totalDepartments = groupedEmployees.Count;
            
            if (departmentsPerPage.HasValue && departmentsPerPage.Value > 0)
            {
                PageSize = departmentsPerPage.Value;
                TotalPages = (int)Math.Ceiling((double)totalDepartments / PageSize);
                
                var departmentsForPage = groupedEmployees
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize);

                Employees = departmentsForPage.SelectMany(g => g);
            }
            else
            {
                Employees = allEmployees;
                TotalPages = 1;
            }
        }
    }
}




