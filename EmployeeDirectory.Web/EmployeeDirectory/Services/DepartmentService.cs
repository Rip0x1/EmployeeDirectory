using Microsoft.EntityFrameworkCore;
using EmployeeDirectory.Data;
using EmployeeDirectory.Models;

namespace EmployeeDirectory.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService? _logService;

        public DepartmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public DepartmentService(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            return await _context.Departments
                .Include(d => d.Head)
                .Include(d => d.Employees)
                    .ThenInclude(e => e.Position)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<Department?> GetDepartmentByIdAsync(int id)
        {
            return await _context.Departments
                .Include(d => d.Head)
                .Include(d => d.Employees)
                    .ThenInclude(e => e.Position)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            department.CreatedAt = DateTime.UtcNow;
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            if (_logService != null)
            {
                await _logService.WriteAsync(new LogEntry
                {
                    Action = "Create",
                    EntityType = nameof(Department),
                    EntityId = department.Id.ToString(),
                    Details = $"Создан отдел: {department.Name}"
                });
            }

            if (department.HeadId.HasValue)
            {
                var employeesInDept = await _context.Employees
                    .Where(e => e.DepartmentId == department.Id)
                    .ToListAsync();
                foreach (var emp in employeesInDept)
                {
                    emp.IsHeadOfDepartment = emp.Id == department.HeadId.Value;
                }

                var head = await _context.Employees.FindAsync(department.HeadId.Value);
                if (head != null)
                {
                    head.DepartmentId = department.Id;
                    head.IsHeadOfDepartment = true;
                }

                await _context.SaveChangesAsync();
            }
            return department;
        }

        public async Task<Department> UpdateDepartmentAsync(Department department)
        {
            department.UpdatedAt = DateTime.UtcNow;
            _context.Departments.Update(department);
            await _context.SaveChangesAsync();

            var employeesInDept = await _context.Employees
                .Where(e => e.DepartmentId == department.Id)
                .ToListAsync();
            foreach (var emp in employeesInDept)
            {
                emp.IsHeadOfDepartment = department.HeadId.HasValue && emp.Id == department.HeadId.Value;
            }
            if (department.HeadId.HasValue)
            {
                var head = await _context.Employees.FindAsync(department.HeadId.Value);
                if (head != null)
                {
                    head.DepartmentId = department.Id;
                    head.IsHeadOfDepartment = true;
                }
            }
            await _context.SaveChangesAsync();
            if (_logService != null)
            {
                await _logService.WriteAsync(new LogEntry
                {
                    Action = "Update",
                    EntityType = nameof(Department),
                    EntityId = department.Id.ToString(),
                    Details = $"Обновлен отдел: {department.Name} (РуководительId={department.HeadId?.ToString() ?? "-"})"
                });
            }
            return department;
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                return false;
            var hasEmployees = await _context.Employees.AnyAsync(e => e.DepartmentId == id);
            if (hasEmployees)
            {
                return false;
            }
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            if (_logService != null)
            {
                await _logService.WriteAsync(new LogEntry
                {
                    Action = "Delete",
                    EntityType = nameof(Department),
                    EntityId = id.ToString(),
                    Details = $"Удален отдел: {department.Name}"
                });
            }
            return true;
        }

        public async Task<IEnumerable<Department>> GetDepartmentsWithHeadsAsync()
        {
            return await _context.Departments
                .Include(d => d.Head)
                .Include(d => d.Employees)
                    .ThenInclude(e => e.Position)
                .Where(d => d.Head != null)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }
    }
}
