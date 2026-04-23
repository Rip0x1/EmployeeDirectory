using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using EmployeeDirectory.Services;
using EmployeeDirectory.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EmployeeDirectory.Data;

namespace EmployeeDirectory.Pages.Departments
{
    [Authorize(Roles = "Administrator")]
    public class ManageModel : PageModel
    {
        private readonly IDepartmentService _departmentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ManageModel(IDepartmentService departmentService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _departmentService = departmentService;
            _userManager = userManager;
            _context = context;
        }

        public IList<Department> Departments { get; set; } = new List<Department>();

        public async Task OnGetAsync()
        {
            Departments = (await _departmentService.GetAllDepartmentsAsync()).ToList();

            foreach (var department in Departments)
            {
                if (department.Head == null)
                {
                    var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
                    var managerForDepartment = managerUsers.FirstOrDefault(u => u.DepartmentId == department.Id);
                    
                    if (managerForDepartment != null)
                    {
                        var employee = await _context.Employees
                            .FirstOrDefaultAsync(e => e.DepartmentId == department.Id && 
                                                       (e.FullName == managerForDepartment.FullName || 
                                                        e.FullName == managerForDepartment.UserName));
                        
                        if (employee != null)
                        {
                            department.HeadId = employee.Id;
                            var display = !string.IsNullOrWhiteSpace(managerForDepartment.FullName)
                                ? $"{managerForDepartment.UserName} ({managerForDepartment.FullName})"
                                : managerForDepartment.UserName ?? employee.FullName;
                            department.Head = new Employee
                            {
                                Id = employee.Id,
                                FullName = display,
                                DepartmentId = employee.DepartmentId,
                                DepartmentName = department.Name ?? ""
                            };
                        }
                        else
                        {
                            var display = !string.IsNullOrWhiteSpace(managerForDepartment.FullName)
                                ? $"{managerForDepartment.UserName} ({managerForDepartment.FullName})"
                                : managerForDepartment.UserName ?? "";
                            department.Head = new Employee
                            {
                                Id = 0,
                                FullName = display,
                                DepartmentId = department.Id,
                                DepartmentName = department.Name ?? ""
                            };
                        }
                    }
                }
            }
        }
    }
}






