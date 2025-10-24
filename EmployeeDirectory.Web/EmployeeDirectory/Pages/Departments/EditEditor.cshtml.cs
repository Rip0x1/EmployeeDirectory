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
    public class EditEditorModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public EditEditorModel(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty]
        public string DepartmentName { get; set; } = string.Empty;
        
        [BindProperty]
        public int DepartmentId { get; set; }
        
        [BindProperty]
        public string EditorId { get; set; } = string.Empty;

        public class InputModel
        {
            [Required(ErrorMessage = "Логин обязателен")]
            [Display(Name = "Логин")]
            public string UserName { get; set; } = string.Empty;

            [Display(Name = "Полное имя")]
            public string FullName { get; set; } = string.Empty;

            [Display(Name = "Новый пароль")]
            [StringLength(100, ErrorMessage = "Пароль должен содержать минимум {2} символов", MinimumLength = 6)]
            public string? NewPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var editor = await _userManager.FindByIdAsync(id);
            if (editor == null)
            {
                return NotFound();
            }

            if (!await _userManager.IsInRoleAsync(editor, "DepartmentEditor"))
            {
                return NotFound();
            }

            EditorId = editor.Id;
            DepartmentId = editor.DepartmentId ?? 0;
            
            if (editor.DepartmentId.HasValue)
            {
                var department = await _context.Departments.FindAsync(editor.DepartmentId.Value);
                DepartmentName = department?.GetDisplayName() ?? "Неизвестный отдел";
            }
            else
            {
                DepartmentName = "Неизвестный отдел";
            }

            Input.UserName = editor.UserName ?? "";
            Input.FullName = editor.FullName;

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (await _userManager.IsInRoleAsync(currentUser, "Administrator"))
            {
                return Page();
            }

            if (await _userManager.IsInRoleAsync(currentUser, "Manager") && 
                currentUser.DepartmentId == editor.DepartmentId)
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
                if (string.IsNullOrEmpty(EditorId))
                {
                    ModelState.AddModelError(string.Empty, "ID редактора не найден");
                    return Page();
                }

                var editor = await _userManager.FindByIdAsync(EditorId);
                if (editor == null)
                {
                    ModelState.AddModelError(string.Empty, "Редактор не найден");
                    return Page();
                }

                editor.UserName = Input.UserName;
                editor.NormalizedUserName = Input.UserName.ToUpper();
                editor.FullName = Input.FullName;

                var result = await _userManager.UpdateAsync(editor);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(Input.NewPassword))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(editor);
                        await _userManager.ResetPasswordAsync(editor, token, Input.NewPassword);
                    }

                    TempData["SuccessMessage"] = "Данные редактора успешно обновлены";
                    return RedirectToPage("/Departments/ManageEditors", new { departmentId = DepartmentId });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка при обновлении редактора: {ex.Message}");
            }

            return Page();
        }
    }
}
