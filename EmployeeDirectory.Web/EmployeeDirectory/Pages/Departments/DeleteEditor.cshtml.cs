using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Models;
using EmployeeDirectory.Data;

namespace EmployeeDirectory.Pages.Departments
{
    [Authorize(Roles = "Manager,Administrator")]
    public class DeleteEditorModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DeleteEditorModel(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public ApplicationUser Editor { get; set; } = new();
        public string DepartmentName { get; set; } = string.Empty;
        public int DepartmentId { get; set; }

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

            Editor = editor;
            DepartmentId = editor.DepartmentId ?? 0;
            DepartmentName = editor.Department?.GetDisplayName() ?? "Неизвестный отдел";

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

        public async Task<IActionResult> OnPostAsync(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var editor = await _userManager.FindByIdAsync(id);
                if (editor == null)
                {
                    return NotFound();
                }

                DepartmentId = editor.DepartmentId ?? 0;

                var result = await _userManager.DeleteAsync(editor);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Редактор отдела успешно удален";
                    return RedirectToPage("/Departments/ManageEditors", new { departmentId = DepartmentId });
                }

                foreach (var error in result.Errors)
                {
                    TempData["ErrorMessage"] = error.Description;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при удалении редактора: {ex.Message}";
            }

            return RedirectToPage("/Departments/ManageEditors", new { departmentId = DepartmentId });
        }
    }
}
