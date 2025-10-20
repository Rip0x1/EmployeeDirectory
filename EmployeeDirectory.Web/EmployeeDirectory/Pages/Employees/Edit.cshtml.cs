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
    public class EditModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IPositionService _positionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(
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
                    TempData["Error"] = "У вас нет прав для редактирования этого сотрудника.";
                    return RedirectToPage("/Index");
                }
            }

            Employee = employee;
            
            // Инициализируем отдельные свойства
            DepartmentId = employee.DepartmentId;
            PositionId = employee.PositionId;

            // Загружаем отделы и должности
            var departments = await _departmentService.GetAllDepartmentsAsync();
            var positions = await _positionService.GetAllPositionsAsync();

            // Если пользователь - начальник отдела, показываем только его отдел
            if (User.IsInRole("Manager") && !User.IsInRole("Administrator"))
            {
                departments = departments.Where(d => d.Id == user.DepartmentId.Value);
            }

            Departments = new SelectList(departments, "Id", "Name", employee.DepartmentId);
            Positions = new SelectList(positions, "Id", "Name", employee.PositionId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Отладочная информация
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Если пользователь - начальник отдела, ограничиваем его отделом
            if (User.IsInRole("Manager") && !User.IsInRole("Administrator"))
            {
                if (user.DepartmentId == null)
                {
                    TempData["Error"] = "У вас не назначен отдел. Обратитесь к администратору.";
                    return RedirectToPage("/Index");
                }
                Employee.DepartmentId = user.DepartmentId.Value;
            }

            // Проверяем валидацию для основных полей
            if (string.IsNullOrEmpty(Employee.PositionDescription))
            {
                ModelState.AddModelError("Employee.PositionDescription", "Должность/описание обязательно");
            }
            
            // Очищаем ошибки валидации для полей, которые мы не используем
            ModelState.Remove("Employee.Department");
            ModelState.Remove("Employee.Position");

            if (!ModelState.IsValid)
            {
                // Перезагружаем данные для формы
                var departments = await _departmentService.GetAllDepartmentsAsync();
                var positions = await _positionService.GetAllPositionsAsync();

                if (User.IsInRole("Manager") && !User.IsInRole("Administrator"))
                {
                    departments = departments.Where(d => d.Id == user.DepartmentId.Value);
                }

                Departments = new SelectList(departments, "Id", "Name", Employee.DepartmentId);
                Positions = new SelectList(positions, "Id", "Name", Employee.PositionId);
                
                // Добавляем ошибки валидации в TempData для отладки
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = $"Ошибки валидации: {string.Join(", ", errors)}";
                return Page();
            }

            try
            {
                // Всегда используем отдельные свойства
                if (User.IsInRole("Manager") && !User.IsInRole("Administrator"))
                {
                    // Для начальников отдела используем их отдел
                    Employee.DepartmentId = user.DepartmentId.Value;
                }
                else
                {
                    // Для администраторов используем выбранный отдел
                    Employee.DepartmentId = DepartmentId;
                }
                Employee.PositionId = null; // Больше не используем PositionId
                
                Employee.UpdatedAt = DateTime.UtcNow;
                await _employeeService.UpdateEmployeeAsync(Employee);
                
                TempData["Success"] = $"Запись {Employee.FullName} успешно обновлена!";
                
                // Определяем предыдущую страницу
                if (!string.IsNullOrEmpty(ReturnUrl))
                {
                    return Redirect(ReturnUrl);
                }
                
                var referer = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer) && !referer.Contains("/Employees/Edit"))
                {
                    return Redirect(referer);
                }
                
                // Если редирект не сработал, возвращаемся на главную
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при обновлении сотрудника: {ex.Message}";
                
                // Перезагружаем данные для формы
                var departments = await _departmentService.GetAllDepartmentsAsync();
                var positions = await _positionService.GetAllPositionsAsync();

                if (User.IsInRole("Manager") && !User.IsInRole("Administrator"))
                {
                    departments = departments.Where(d => d.Id == user.DepartmentId.Value);
                }

                Departments = new SelectList(departments, "Id", "Name", Employee.DepartmentId);
                Positions = new SelectList(positions, "Id", "Name", Employee.PositionId);
                return Page();
            }
        }
    }
}
