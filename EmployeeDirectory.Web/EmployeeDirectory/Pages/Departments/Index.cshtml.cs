using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Services;
using EmployeeDirectory.Models;
using Microsoft.AspNetCore.Identity;

namespace EmployeeDirectory.Pages.Departments
{
    [Authorize(Roles = "Manager,Administrator")]
    public class IndexModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(IEmployeeService employeeService, IDepartmentService departmentService, UserManager<ApplicationUser> userManager)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _userManager = userManager;
        }

        public IEnumerable<Employee> Employees { get; set; } = new List<Employee>();
        public string DepartmentName { get; set; } = string.Empty;
        public int DepartmentId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            if (user.DepartmentId == null)
            {
                TempData["Error"] = "У вас не назначен отдел. Обратитесь к администратору.";
                return RedirectToPage("/Index");
            }

            DepartmentId = user.DepartmentId.Value;
            var department = await _departmentService.GetDepartmentByIdAsync(DepartmentId);
            if (department == null)
            {
                TempData["Error"] = "Отдел не найден.";
                return RedirectToPage("/Index");
            }

            DepartmentName = department.Name;
            Employees = await _employeeService.GetEmployeesByDepartmentAsync(DepartmentId);

            return Page();
        }
    }
}
