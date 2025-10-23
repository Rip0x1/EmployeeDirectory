using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EmployeeDirectory.Models;
using EmployeeDirectory.Data;

namespace EmployeeDirectory.Services
{
    public class DepartmentEditorService : IDepartmentEditorService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DepartmentEditorService(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IdentityResult> CreateDepartmentEditorAsync(string userName, string password, string fullName, int departmentId)
        {
            // Проверяем, что отдел существует
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Отдел не найден" });
            }

            // Проверяем, что пользователь с таким логином не существует
            var existingUser = await _userManager.FindByNameAsync(userName);
            if (existingUser != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Пользователь с таким логином уже существует" });
            }

            // Создаем нового пользователя
            var user = new ApplicationUser
            {
                UserName = userName,
                FullName = fullName,
                DepartmentId = departmentId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            
            if (result.Succeeded)
            {
                // Назначаем роль "Редактор отдела"
                await _userManager.AddToRoleAsync(user, "DepartmentEditor");
            }

            return result;
        }

        public async Task<List<ApplicationUser>> GetDepartmentEditorsAsync(int departmentId)
        {
            // Получаем всех пользователей отдела
            var users = await _context.Users
                .Where(u => u.DepartmentId == departmentId)
                .ToListAsync();

            // Фильтруем на клиенте по роли
            var editors = new List<ApplicationUser>();
            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "DepartmentEditor"))
                {
                    editors.Add(user);
                }
            }

            return editors;
        }

        public async Task<bool> CanUserEditDepartmentAsync(string userId, int departmentId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Администратор может редактировать любой отдел
            if (await _userManager.IsInRoleAsync(user, "Administrator"))
                return true;

            // Начальник отдела может редактировать свой отдел
            if (await _userManager.IsInRoleAsync(user, "Manager") && user.DepartmentId == departmentId)
                return true;

            // Редактор отдела может редактировать только свой отдел
            if (await _userManager.IsInRoleAsync(user, "DepartmentEditor") && user.DepartmentId == departmentId)
                return true;

            return false;
        }

        public async Task<bool> IsDepartmentEditorAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            return await _userManager.IsInRoleAsync(user, "DepartmentEditor");
        }
    }
}
