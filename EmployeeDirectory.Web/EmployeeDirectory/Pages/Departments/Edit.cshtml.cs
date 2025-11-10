using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Models;
using EmployeeDirectory.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EmployeeDirectory.Data;

namespace EmployeeDirectory.Pages.Departments
{
    [Authorize(Roles = "Administrator")]
    public class EditModel : PageModel
    {
        private readonly IDepartmentService _departmentService;
        private readonly IEmployeeService _employeeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public EditModel(IDepartmentService departmentService, IEmployeeService employeeService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _departmentService = departmentService;
            _employeeService = employeeService;
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public Department Department { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public SelectList Employees { get; set; } = default!;

        private async Task LoadEmployeesAsync()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            var employeeOptions = employees
                .Select(e => new {
                    Id = e.Id,
                    Label = string.IsNullOrWhiteSpace(e.FullName) ? (e.PositionDescription ?? "") : e.FullName
                })
                .Where(o => !string.IsNullOrWhiteSpace(o.Label))
                .ToList();

            var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
            
            foreach (var user in managerUsers)
            {
                var tempId = -(user.Id.GetHashCode() & 0x7FFFFFFF);
                var label = !string.IsNullOrWhiteSpace(user.FullName)
                    ? $"{user.UserName} ({user.FullName})"
                    : user.UserName ?? "";
                if (!string.IsNullOrWhiteSpace(label))
                {
                    employeeOptions.Add(new { Id = tempId, Label = label });
                }
            }

            Employees = new SelectList(employeeOptions.OrderBy(o => o.Label), "Id", "Label");
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            Department = department;
            await LoadEmployeesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadEmployeesAsync();
                return Page();
            }

            var selectedManagerUserId = (string?)null;
            if (Department.HeadId.HasValue && Department.HeadId < 0)
            {
                var tempId = Department.HeadId.Value;
                var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
                foreach (var user in managerUsers)
                {
                    var calculatedTempId = -(user.Id.GetHashCode() & 0x7FFFFFFF);
                    if (calculatedTempId == tempId)
                    {
                        selectedManagerUserId = user.Id;
                        break;
                    }
                }
                Department.HeadId = null;
            }

            await _departmentService.UpdateDepartmentAsync(Department);

            if (!string.IsNullOrEmpty(selectedManagerUserId))
            {
                var managerUser = await _userManager.FindByIdAsync(selectedManagerUserId);
                if (managerUser != null)
                {
                    managerUser.DepartmentId = Department.Id;
                    await _userManager.UpdateAsync(managerUser);
                }
            }

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


