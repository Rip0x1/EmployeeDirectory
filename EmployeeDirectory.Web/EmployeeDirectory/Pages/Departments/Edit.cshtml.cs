using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Models;
using EmployeeDirectory.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeDirectory.Pages.Departments
{
    [Authorize(Roles = "Administrator")]
    public class EditModel : PageModel
    {
        private readonly IDepartmentService _departmentService;
        private readonly IEmployeeService _employeeService;

        public EditModel(IDepartmentService departmentService, IEmployeeService employeeService)
        {
            _departmentService = departmentService;
            _employeeService = employeeService;
        }

        [BindProperty]
        public Department Department { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public SelectList Employees { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            Department = department;
            var employees = await _employeeService.GetAllEmployeesAsync();
            var options = employees
                .Select(e => new {
                    Id = e.Id,
                    Label = string.IsNullOrWhiteSpace(e.FullName) ? (e.PositionDescription ?? "") : e.FullName
                })
                .Where(o => !string.IsNullOrWhiteSpace(o.Label))
                .ToList();
            Employees = new SelectList(options, "Id", "Label");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var employees = await _employeeService.GetAllEmployeesAsync();
                var options = employees
                    .Select(e => new {
                        Id = e.Id,
                        Label = string.IsNullOrWhiteSpace(e.FullName) ? (e.PositionDescription ?? "") : e.FullName
                    })
                    .Where(o => !string.IsNullOrWhiteSpace(o.Label))
                    .ToList();
                Employees = new SelectList(options, "Id", "Label");
                return Page();
            }

            await _departmentService.UpdateDepartmentAsync(Department);

            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && !referer.Contains("/Departments/Edit"))
            {
                return Redirect(referer);
            }

            return RedirectToPage("/Departments/Manage");
        }
    }
}


