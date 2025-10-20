using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Services;
using EmployeeDirectory.Models;
using Microsoft.AspNetCore.Identity;

namespace EmployeeDirectory.Pages.Employees
{
    [Authorize(Roles = "Manager,Administrator")]
    public class DeleteModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteModel(IEmployeeService employeeService, UserManager<ApplicationUser> userManager)
        {
            _employeeService = employeeService;
            _userManager = userManager;
        }

        public Employee Employee { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                TempData["Error"] = "Сотрудник не найден.";
                return RedirectToPage("/Index");
            }

            // Если пользователь - начальник отдела, проверяем, что сотрудник из его отдела
            if (User.IsInRole("Manager") && !User.IsInRole("Administrator"))
            {
                if (user.DepartmentId == null || employee.DepartmentId != user.DepartmentId.Value)
                {
                    TempData["Error"] = "У вас нет прав для удаления этого сотрудника.";
                    return RedirectToPage("/Index");
                }
            }

            Employee = employee;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                TempData["Error"] = "Сотрудник не найден.";
                return RedirectToPage("/Index");
            }

            // Если пользователь - начальник отдела, проверяем, что сотрудник из его отдела
            if (User.IsInRole("Manager") && !User.IsInRole("Administrator"))
            {
                if (user.DepartmentId == null || employee.DepartmentId != user.DepartmentId.Value)
                {
                    TempData["Error"] = "У вас нет прав для удаления этого сотрудника.";
                    return RedirectToPage("/Index");
                }
            }

            try
            {
                var result = await _employeeService.DeleteEmployeeAsync(id);
                if (result)
                {
                    TempData["Success"] = $"Сотрудник {employee.FullName} успешно удален!";
                }
                else
                {
                    TempData["Error"] = "Ошибка при удалении сотрудника.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при удалении сотрудника: {ex.Message}";
            }

            // Определяем предыдущую страницу
            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }
            
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && !referer.Contains("/Employees/Delete"))
            {
                return Redirect(referer);
            }
            
            // Если редирект не сработал, возвращаемся на главную
            return RedirectToPage("/Index");
        }
    }
}

