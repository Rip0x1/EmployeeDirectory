using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Models;

namespace EmployeeDirectory.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Показываем страницу подтверждения выхода
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userName = User.Identity.Name;
                _logger.LogInformation("Пользователь {Name} вышел из системы", userName);
                
                // Выполняем выход из системы
                await _signInManager.SignOutAsync();
                
                TempData["Success"] = $"Вы успешно вышли из системы. До свидания, {userName}!";
            }

            // Перенаправляем на главную страницу после выхода
            return RedirectToPage("/Index");
        }
    }
}