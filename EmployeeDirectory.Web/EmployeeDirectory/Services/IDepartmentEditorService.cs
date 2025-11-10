using EmployeeDirectory.Models;
using Microsoft.AspNetCore.Identity;

namespace EmployeeDirectory.Services
{
    public interface IDepartmentEditorService
    {
        Task<IdentityResult> CreateDepartmentEditorAsync(string userName, string fullName, int departmentId);
        Task<List<ApplicationUser>> GetDepartmentEditorsAsync(int departmentId);
        Task<bool> CanUserEditDepartmentAsync(string userId, int departmentId);
        Task<bool> IsDepartmentEditorAsync(string userId);
    }
}
