using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Models;

namespace EmployeeDirectory.Pages.Users
{
    [Authorize(Roles = "Administrator")]
    public class DeleteModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public string UserId { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public ApplicationUser? UserToDelete { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            UserToDelete = await _userManager.FindByIdAsync(id);
            if (UserToDelete == null)
            {
                return NotFound();
            }

            UserId = id;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(user.UserName) && user.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Нельзя удалить основного администратора системы.";
                return RedirectToPage("/Users/Manage");
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = $"Пользователь {user.UserName} успешно удален!";
            }
            else
            {
                TempData["Error"] = "Ошибка при удалении пользователя.";
            }

            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && !referer.Contains("/Users/Delete"))
            {
                return Redirect(referer);
            }

            return RedirectToPage("/Users/Manage");
        }
    }
}
