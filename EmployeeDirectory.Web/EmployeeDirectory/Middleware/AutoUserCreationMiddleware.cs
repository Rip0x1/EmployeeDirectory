using Microsoft.AspNetCore.Identity;
using EmployeeDirectory.Models;
using EmployeeDirectory.Data;
using EmployeeDirectory.Services;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDirectory.Middleware
{
    public class AutoUserCreationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AutoUserCreationMiddleware> _logger;

        public AutoUserCreationMiddleware(RequestDelegate next, ILogger<AutoUserCreationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext dbContext,
            SignInManager<ApplicationUser> signInManager,
            ILoginLogService loginLogService)
        {
            if (context.User?.Identity?.IsAuthenticated == true && context.User.Identity.Name != null)
            {
                var windowsUsername = context.User.Identity.Name;
                
                var existingUser = await userManager.FindByNameAsync(windowsUsername);
                var ipAddress = GetClientIpAddress(context);
                var userAgent = context.Request.Headers["User-Agent"].ToString();
                
                if (existingUser == null)
                {
                    await EnsureRolesExistAsync(roleManager);
                    
                    var windowsUsersCount = await dbContext.Users
                        .Where(u => u.UserName != null && u.UserName != "admin")
                        .CountAsync();
                    
                    var isFirstUser = windowsUsersCount == 0;
                    
                    var newUser = new ApplicationUser
                    {
                        UserName = windowsUsername,
                        FullName = windowsUsername,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var createResult = await userManager.CreateAsync(newUser);
                    
                    if (createResult.Succeeded)
                    {
                        if (isFirstUser)
                        {
                            await userManager.AddToRoleAsync(newUser, "Administrator");
                            _logger.LogInformation("Создан первый пользователь {UserName} с правами администратора", windowsUsername);
                            
                            newUser = await userManager.FindByIdAsync(newUser.Id);
                            if (newUser != null)
                            {
                                await signInManager.SignInAsync(newUser, isPersistent: false);
                                await signInManager.RefreshSignInAsync(newUser);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Создан новый пользователь {UserName}", windowsUsername);
                            await signInManager.SignInAsync(newUser, isPersistent: false);
                        }
                        
                        await loginLogService.WriteLoginLogAsync(new LoginLog
                        {
                            UserName = windowsUsername,
                            Action = "Login",
                            IpAddress = ipAddress,
                            UserAgent = userAgent,
                            Success = true
                        });
                    }
                    else
                    {
                        _logger.LogError("Не удалось создать пользователя {UserName}: {Errors}", 
                            windowsUsername, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                        
                        await loginLogService.WriteLoginLogAsync(new LoginLog
                        {
                            UserName = windowsUsername,
                            Action = "FailedLogin",
                            IpAddress = ipAddress,
                            UserAgent = userAgent,
                            Success = false,
                            FailureReason = $"Ошибка создания пользователя: {string.Join(", ", createResult.Errors.Select(e => e.Description))}"
                        });
                    }
                }
                else
                {
                    if (!signInManager.IsSignedIn(context.User))
                    {
                        await signInManager.SignInAsync(existingUser, isPersistent: false);
                    }
                    else
                    {
                        await signInManager.RefreshSignInAsync(existingUser);
                    }
                    
                    var logExists = await dbContext.LoginLogs
                        .Where(l => l.UserName == windowsUsername && 
                                    l.Action == "Login" && 
                                    l.TimestampUtc >= DateTime.UtcNow.AddMinutes(-5))
                        .AnyAsync();
                    
                    if (!logExists)
                    {
                        await loginLogService.WriteLoginLogAsync(new LoginLog
                        {
                            UserName = windowsUsername,
                            Action = "Login",
                            IpAddress = ipAddress,
                            UserAgent = userAgent,
                            Success = true
                        });
                    }
                }
            }

            await _next(context);
        }
        
        private string GetClientIpAddress(HttpContext context)
        {
            var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Connection.RemoteIpAddress?.ToString();
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

        private async Task EnsureRolesExistAsync(RoleManager<ApplicationRole> roleManager)
        {
            string[] roles = { "Manager", "Administrator", "DepartmentEditor" };
            
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var description = roleName switch
                    {
                        "Manager" => "Начальник отдела",
                        "Administrator" => "Администратор",
                        "DepartmentEditor" => "Редактор отдела",
                        _ => roleName
                    };

                    var role = new ApplicationRole
                    {
                        Name = roleName,
                        Description = description
                    };
                    await roleManager.CreateAsync(role);
                }
            }
        }
    }
}

