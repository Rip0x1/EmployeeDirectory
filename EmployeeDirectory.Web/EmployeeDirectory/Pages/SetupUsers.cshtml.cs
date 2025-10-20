using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using EmployeeDirectory.Models;
using EmployeeDirectory.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDirectory.Pages
{
    public class SetupUsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public SetupUsersModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public bool UsersCreated { get; set; } = false;

        public async Task<IActionResult> OnGetAsync()
        {
            var adminExists = await _userManager.FindByNameAsync("admin");
            var managerExists = await _userManager.FindByNameAsync("manager");
            
            UsersCreated = adminExists != null && managerExists != null;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var departments = await _context.Departments.ToListAsync();
                if (!departments.Any())
                {
                    TempData["Error"] = "Сначала создайте отделы через инициализацию тестовых данных.";
                    return Page();
                }

                var firstDepartment = departments.First();

                var adminUser = await _userManager.FindByNameAsync("admin");
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = "admin",
                        Email = "admin@company.com",
                        FullName = "Администратор системы",
                        DepartmentId = firstDepartment.Id,
                        IsActive = true
                    };

                    await _userManager.CreateAsync(adminUser, "admin123");
                    await _userManager.AddToRoleAsync(adminUser, "Administrator");
                }

                var managerUser = await _userManager.FindByNameAsync("manager");
                if (managerUser == null)
                {
                    managerUser = new ApplicationUser
                    {
                        UserName = "manager",
                        Email = "manager@company.com",
                        FullName = "Начальник IT отдела",
                        DepartmentId = firstDepartment.Id,
                        IsActive = true
                    };

                    await _userManager.CreateAsync(managerUser, "manager123");
                    await _userManager.AddToRoleAsync(managerUser, "Manager");
                }

                UsersCreated = true;
                TempData["Success"] = "Пользователи успешно созданы!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при создании пользователей: {ex.Message}";
            }

            return Page();
        }
    }
}


