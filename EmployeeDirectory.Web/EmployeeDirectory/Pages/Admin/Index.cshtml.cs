using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Services;
using Microsoft.AspNetCore.Identity;
using EmployeeDirectory.Models;

namespace EmployeeDirectory.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class IndexModel : PageModel
    {
        private readonly IDepartmentService _departmentService;
        private readonly IEmployeeService _employeeService;
        private readonly IPositionService _positionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            IDepartmentService departmentService,
            IEmployeeService employeeService,
            IPositionService positionService,
            UserManager<ApplicationUser> userManager)
        {
            _departmentService = departmentService;
            _employeeService = employeeService;
            _positionService = positionService;
            _userManager = userManager;
        }

        public int TotalDepartments { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalPositions { get; set; }
        public int TotalUsers { get; set; }

        public async Task OnGetAsync()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            var employees = await _employeeService.GetAllEmployeesAsync();
            var positions = await _positionService.GetAllPositionsAsync();
            var users = _userManager.Users.ToList();

            TotalDepartments = departments.Count();
            TotalEmployees = employees.Count();
            TotalPositions = positions.Count();
            TotalUsers = users.Count;
        }
    }
}