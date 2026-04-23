using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Models;
using EmployeeDirectory.Services;
using System.ComponentModel.DataAnnotations;

namespace EmployeeDirectory.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ILoginLogService _loginLogService;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<LoginModel> logger, ILoginLogService loginLogService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _loginLogService = loginLogService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Логин обязателен")]
            [Display(Name = "Логин")]
            public string UserName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Пароль обязателен")]
            [DataType(DataType.Password)]
            [Display(Name = "Пароль")]
            public string Password { get; set; } = string.Empty;
        }

        public Task OnGetAsync(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                Response.Redirect("/");
                return Task.CompletedTask;
            }

            ReturnUrl = returnUrl;
            return Task.CompletedTask;
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

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var ipAddress = GetClientIpAddress();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                
                var user = await _userManager.FindByNameAsync(Input.UserName);
                if (user != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: false);
                    
                    if (result.Succeeded)
                    {
                        var properties = new AuthenticationProperties
                        {
                            IsPersistent = false,
                            ExpiresUtc = null
                        };
                        
                        await _signInManager.SignInAsync(user, properties);
                        
                        _logger.LogInformation("Пользователь {UserName} вошел в систему", Input.UserName);
                        
                        await _loginLogService.WriteLoginLogAsync(new LoginLog
                        {
                            UserName = Input.UserName,
                            Action = "Login",
                            IpAddress = ipAddress,
                            UserAgent = userAgent,
                            Success = true
                        });
                        
                        return LocalRedirect(returnUrl);
                    }
                }
                
                await _loginLogService.WriteLoginLogAsync(new LoginLog
                {
                    UserName = Input.UserName,
                    Action = "FailedLogin",
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Success = false,
                    FailureReason = "Ошибка аутентификации"
                });
                
                ModelState.AddModelError(string.Empty, "Неверный логин или пароль");
            }

            return Page();
        }
    }
}