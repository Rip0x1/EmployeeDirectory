using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using EmployeeDirectory.Services;
using EmployeeDirectory.Models;
using Microsoft.AspNetCore.Identity;

namespace EmployeeDirectory.Pages.Employees
{
    [Authorize(Roles = "Manager,Administrator")]
    public class CreateModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IPositionService _positionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            IPositionService positionService,
            UserManager<ApplicationUser> userManager)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _positionService = positionService;
            _userManager = userManager;
        }

        [BindProperty]
        public Employee Employee { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        [BindProperty]
        public int DepartmentId { get; set; }

        [BindProperty]
        public int? PositionId { get; set; }

        public SelectList Departments { get; set; } = new(new List<Department>(), "Id", "Name");
        public SelectList Positions { get; set; } = new(new List<Position>(), "Id", "Name");

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var departments = await _departmentService.GetAllDepartmentsAsync();
            var positions = await _positionService.GetAllPositionsAsync();

            if (User.IsInRole("Manager") && !User.IsInRole("Administrator"))
            {
                if (user.DepartmentId == null)
                {
                    TempData["Error"] = "У вас не назначен отдел. Обратитесь к администратору.";
                    return RedirectToPage("/Index");
                }
                departments = departments.Where(d => d.Id == user.DepartmentId.Value);
            }

            Departments = new SelectList(departments, "Id", "Name");
            Positions = new SelectList(positions, "Id", "Name");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            if (User.IsInRole("Manager") && !User.IsInRole("Administrator"))
            {
                if (user.DepartmentId == null)
                {
                    TempData["Error"] = "У вас не назначен отдел. Обратитесь к администратору.";
                    return RedirectToPage("/Index");
                }
                Employee.DepartmentId = user.DepartmentId.Value;
            }

            if (string.IsNullOrEmpty(Employee.PositionDescription))
            {
                ModelState.AddModelError("Employee.PositionDescription", "Должность/описание обязательно");
            }
            
            ModelState.Remove("Employee.Department");
            ModelState.Remove("Employee.Position");

            if (!ModelState.IsValid)
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();
                var positions = await _positionService.GetAllPositionsAsync();

                if (User.IsInRole("Manager") && !User.IsInRole("Administrator"))
                {
                    departments = departments.Where(d => d.Id == user.DepartmentId.Value);
                }

                Departments = new SelectList(departments, "Id", "Name");
                Positions = new SelectList(positions, "Id", "Name");
                
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = $"Ошибки валидации: {string.Join(", ", errors)}";
                return Page();
            }

            try
            {
                if (User.IsInRole("Manager") && !User.IsInRole("Administrator"))
                {
                    Employee.DepartmentId = user.DepartmentId.Value;
                }
                else
                {
                    Employee.DepartmentId = DepartmentId;
                }
                Employee.PositionId = null; 

                Employee.CreatedAt = DateTime.UtcNow;
                await _employeeService.AddEmployeeAsync(Employee);
                
                TempData["Success"] = $"Запись {Employee.FullName} успешно добавлена!";
                
                if (!string.IsNullOrEmpty(ReturnUrl))
                {
                    return Redirect(ReturnUrl);
                }
                
                var referer = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer) && !referer.Contains("/Employees/Create"))
                {
                    return Redirect(referer);
                }
                
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при добавлении сотрудника: {ex.Message}";
                
                var departments = await _departmentService.GetAllDepartmentsAsync();
                var positions = await _positionService.GetAllPositionsAsync();

                if (User.IsInRole("Manager") && !User.IsInRole("Administrator"))
                {
                    departments = departments.Where(d => d.Id == user.DepartmentId.Value);
                }

                Departments = new SelectList(departments, "Id", "Name");
                Positions = new SelectList(positions, "Id", "Name");
                return Page();
            }
        }
    }
}
