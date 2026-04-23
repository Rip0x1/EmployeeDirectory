using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using EmployeeDirectory.Services;
using EmployeeDirectory.Models;

namespace EmployeeDirectory.Pages.Departments
{
    [Authorize(Roles = "Administrator")]
    public class ManageModel : PageModel
    {
        private readonly IDepartmentService _departmentService;

        public ManageModel(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        public IList<Department> Departments { get; set; } = new List<Department>();

        public async Task OnGetAsync()
        {
            Departments = (await _departmentService.GetAllDepartmentsAsync()).ToList();
        }
    }
}






