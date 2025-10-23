using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Services;
using EmployeeDirectory.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeDirectory.Pages.Print;

[Authorize(Roles = "Manager,Administrator,DepartmentEditor")]
public class PrintDepartmentsModel : PageModel
{
    private readonly IDepartmentService _departmentService;
    private readonly IEmployeeService _employeeService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PrintDepartmentsModel(IDepartmentService departmentService, IEmployeeService employeeService, UserManager<ApplicationUser> userManager)
    {
        _departmentService = departmentService;
        _employeeService = employeeService;
        _userManager = userManager;
    }

    public IEnumerable<Employee> Employees { get; set; } = new List<Employee>();
    public Department? Department { get; set; }
    public string DepartmentName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string? orientation = "portrait", int? scale = 100)
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

        Department = await _departmentService.GetDepartmentByIdAsync(user.DepartmentId.Value);
        if (Department == null)
        {
            TempData["Error"] = $"Отдел с ID {user.DepartmentId} не найден в базе данных. Обратитесь к администратору.";
            return RedirectToPage("/Index");
        }

        DepartmentName = Department.Name;
        Employees = await _employeeService.GetEmployeesByDepartmentAsync(user.DepartmentId.Value);
        
        ViewData["Orientation"] = orientation ?? "portrait";
        ViewData["Scale"] = scale ?? 100;
        
        return Page();
    }
}