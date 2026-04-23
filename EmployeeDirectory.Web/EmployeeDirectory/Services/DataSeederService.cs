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
            await CreateRolesAsync();
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
    }
}
