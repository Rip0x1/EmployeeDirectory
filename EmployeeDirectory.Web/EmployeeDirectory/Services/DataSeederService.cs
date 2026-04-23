using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EmployeeDirectory.Data;
using EmployeeDirectory.Models;

namespace EmployeeDirectory.Services
{
    public class DataSeederService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogService _logService;

        public DataSeederService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogService logService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logService = logService;
        }

        public async Task SeedDataAsync()
        {
            await _logService.WriteAsync(new LogEntry
            {
                Action = "SYSTEM_INIT",
                EntityType = "System",
                Details = "Начало инициализации системы"
            });

            await CreateRolesAsync();
            await CreateAdminUserAsync();

            await _logService.WriteAsync(new LogEntry
            {
                Action = "SYSTEM_INIT",
                EntityType = "System",
                Details = "Инициализация системы завершена"
            });
        }

        private async Task CreateRolesAsync()
        {
            var roles = new[] { "Administrator", "Manager", "DepartmentEditor" };
            
            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new ApplicationRole
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper(),
                        Description = GetRoleDescription(roleName),
                        CreatedAt = DateTime.UtcNow
                    };
                    await _roleManager.CreateAsync(role);

                    await _logService.WriteAsync(new LogEntry
                    {
                        Action = "CREATE",
                        EntityType = "Role",
                        EntityId = role.Id,
                        Details = $"Создана роль: {roleName}"
                    });
                }
            }
        }

        private string GetRoleDescription(string roleName)
        {
            return roleName switch
            {
                "Administrator" => "Администратор системы",
                "Manager" => "Менеджер",
                "DepartmentEditor" => "Редактор отдела",
                _ => "Пользователь"
            };
        }

        private async Task CreateAdminUserAsync()
        {
            var adminUser = await _userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    Email = "admin@company.com",
                    NormalizedEmail = "ADMIN@COMPANY.COM",
                    EmailConfirmed = true,
                    FullName = "Администратор Системы",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(adminUser, "admin123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Administrator");

                    await _logService.WriteAsync(new LogEntry
                    {
                        Action = "CREATE",
                        EntityType = "User",
                        EntityId = adminUser.Id,
                        Details = "Создан администратор: admin"
                    });
                }
            }
        }

    }
}
