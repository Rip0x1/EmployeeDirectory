using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Models;
using EmployeeDirectory.Services;

namespace EmployeeDirectory.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly ILoginLogService _loginLogService;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger, ILoginLogService loginLogService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _loginLogService = loginLogService;
        }

        private string GetClientIpAddress()
        {
            var ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            }
            else
            {
                var ips = ipAddress.Split(',');
                ipAddress = ips[0].Trim();
            }
            
            if (ipAddress == "::1" || ipAddress == "127.0.0.1")
            {
                ipAddress = "Локальный адрес (::1)";
            }
            
            return ipAddress ?? "Не определен";
        }

        public Task<IActionResult> OnGetAsync()
        {
            return Task.FromResult<IActionResult>(Page());
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userName = User.Identity.Name;
                _logger.LogInformation("Пользователь {Name} вышел из системы", userName);
                
                var ipAddress = GetClientIpAddress();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                
                await _loginLogService.WriteLoginLogAsync(new LoginLog
                {
                    UserName = userName,
                    Action = "Logout",
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Success = true
                });
                
                await _signInManager.SignOutAsync();
                
                TempData["Success"] = $"Вы успешно вышли из системы. До свидания, {userName}!";
            }

            return RedirectToPage("/Index");
        }
    }
}