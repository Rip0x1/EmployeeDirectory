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

        [BindProperty]
        public string EditorId { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public ApplicationUser? EditorToDelete { get; set; }
        public int DepartmentId { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var editor = await _userManager.FindByIdAsync(id);
            if (editor == null)
            {
                return NotFound();
            }

            if (!await _userManager.IsInRoleAsync(editor, "DepartmentEditor"))
            {
                return NotFound();
            }

            EditorToDelete = editor;
            DepartmentId = editor.DepartmentId ?? 0;
            EditorId = id;

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
            var editor = await _userManager.FindByIdAsync(EditorId);
            if (editor == null)
            {
                return NotFound();
            }

            DepartmentId = editor.DepartmentId ?? 0;

            var result = await _userManager.DeleteAsync(editor);

            if (result.Succeeded)
            {
                TempData["Success"] = $"Редактор {editor.UserName} успешно удален!";
            }
            else
            {
                TempData["Error"] = "Ошибка при удалении редактора.";
            }

            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && !referer.Contains("/Departments/DeleteEditor"))
            {
                return Redirect(referer);
            }

            return RedirectToPage("/Departments/ManageEditors", new { departmentId = DepartmentId });
        }
    }
}
