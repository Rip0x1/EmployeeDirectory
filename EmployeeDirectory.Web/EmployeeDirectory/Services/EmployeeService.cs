using Microsoft.EntityFrameworkCore;
using EmployeeDirectory.Data;
using EmployeeDirectory.Models;

namespace EmployeeDirectory.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService? _logService;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public EmployeeService(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .OrderBy(e => e.Department.Name)
                .ThenBy(e => e.IsHeadOfDepartment ? 0 : 1)
                .ThenBy(e => e.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(int departmentId)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Where(e => e.DepartmentId == departmentId)
                .OrderBy(e => e.IsHeadOfDepartment ? 0 : 1)
                .ThenBy(e => e.FullName)
                .ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            employee.CreatedAt = DateTime.UtcNow;
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            if (_logService != null)
            {
                await _logService.WriteAsync(new LogEntry
                {
                    Action = "Create",
                    EntityType = nameof(Employee),
                    EntityId = employee.Id.ToString(),
                    Details = $"Создан сотрудник: {employee.FullName ?? employee.PositionDescription}"
                });
            }
            return employee;
        }

        public async Task AddEmployeeAsync(Employee employee)
        {
            employee.CreatedAt = DateTime.UtcNow;
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            if (employee.IsHeadOfDepartment && employee.DepartmentId > 0)
            {
                var department = await _context.Departments.FindAsync(employee.DepartmentId);
                if (department != null)
                {
                    department.HeadId = employee.Id;
                    await _context.SaveChangesAsync();
                }
            }
            if (_logService != null)
            {
                await _logService.WriteAsync(new LogEntry
                {
                    Action = "Create",
                    EntityType = nameof(Employee),
                    EntityId = employee.Id.ToString(),
                    Details = $"Добавлен сотрудник: {employee.FullName ?? employee.PositionDescription}"
                });
            }
        }

        public async Task<Employee> UpdateEmployeeAsync(Employee employee)
        {
            employee.UpdatedAt = DateTime.UtcNow;
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();

            if (employee.IsHeadOfDepartment && employee.DepartmentId > 0)
            {
                var department = await _context.Departments.FindAsync(employee.DepartmentId);
                if (department != null)
                {
                    department.HeadId = employee.Id;
                    await _context.SaveChangesAsync();
                }
            }

            if (_logService != null)
            {
                await _logService.WriteAsync(new LogEntry
                {
                    Action = "Update",
                    EntityType = nameof(Employee),
                    EntityId = employee.Id.ToString(),
                    Details = $"Обновлен сотрудник: {employee.FullName ?? employee.PositionDescription}"
                });
            }
            return employee;
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return false;

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            if (_logService != null)
            {
                await _logService.WriteAsync(new LogEntry
                {
                    Action = "Delete",
                    EntityType = nameof(Employee),
                    EntityId = id.ToString(),
                    Details = $"Удален сотрудник: {employee.FullName ?? employee.PositionDescription}"
                });
            }
            return true;
        }

        public async Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllEmployeesAsync();

            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Where(e => e.FullName.Contains(searchTerm) ||
                           e.CityPhone!.Contains(searchTerm) ||
                           e.LocalPhone!.Contains(searchTerm) ||
                           e.Department.Name.Contains(searchTerm))
                .OrderBy(e => e.Department.Name)
                .ThenBy(e => e.IsHeadOfDepartment ? 0 : 1)
                .ThenBy(e => e.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> FilterEmployeesAsync(int? departmentId, bool? isHead)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .AsQueryable();

            if (departmentId.HasValue)
                query = query.Where(e => e.DepartmentId == departmentId.Value);

            if (isHead.HasValue)
                query = query.Where(e => e.IsHeadOfDepartment == isHead.Value);

            return await query
                .OrderBy(e => e.Department.Name)
                .ThenBy(e => e.IsHeadOfDepartment ? 0 : 1)
                .ThenBy(e => e.FullName)
                .ToListAsync();
        }
    }
}
