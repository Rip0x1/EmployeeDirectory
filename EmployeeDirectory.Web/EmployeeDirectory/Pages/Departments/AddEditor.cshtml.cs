using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EmployeeDirectory.Models;
using EmployeeDirectory.Services;
using System.ComponentModel.DataAnnotations;
using EmployeeDirectory.Data;

namespace EmployeeDirectory.Pages.Departments
{
    [Authorize(Roles = "Manager,Administrator")]
    public class AddEditorModel : PageModel
    {
        private readonly IDepartmentEditorService _editorService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AddEditorModel(
            IDepartmentEditorService editorService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _editorService = editorService;
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty]
        public string DepartmentName { get; set; } = string.Empty;
        
        [BindProperty]
        public int DepartmentId { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Логин обязателен")]
            [Display(Name = "Логин")]
            public string UserName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Пароль обязателен")]
            [StringLength(100, ErrorMessage = "Пароль должен содержать минимум {2} символов", MinimumLength = 6)]
            [Display(Name = "Пароль")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Полное имя обязательно")]
            [Display(Name = "Полное имя")]
            public string FullName { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(int? departmentId)
        {
            if (departmentId == null || departmentId <= 0)
            {
                TempData["ErrorMessage"] = "Не указан ID отдела";
                return RedirectToPage("/Departments/Index");
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == departmentId);
            if (department == null)
            {
                TempData["ErrorMessage"] = $"Отдел с ID {departmentId} не найден в базе данных";
                return RedirectToPage("/Departments/Index");
            }

            DepartmentId = department.Id;
            DepartmentName = department.GetDisplayName();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (await _userManager.IsInRoleAsync(currentUser, "Administrator"))
            {
                return Page();
            }

            if (await _userManager.IsInRoleAsync(currentUser, "Manager") && currentUser.DepartmentId == departmentId)
            {
                return Page();
            }

            return Forbid();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var result = await _editorService.CreateDepartmentEditorAsync(
                    Input.UserName,
                    Input.Password,
                    Input.FullName,
                    DepartmentId);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Редактор отдела успешно создан";
                    return RedirectToPage("/Departments/Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка при создании редактора: {ex.Message}");
            }

            return Page();
        }
    }
}
