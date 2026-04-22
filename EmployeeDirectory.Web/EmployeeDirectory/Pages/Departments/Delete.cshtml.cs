using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Models;
using EmployeeDirectory.Services;

namespace EmployeeDirectory.Pages.Departments
{
    [Authorize(Roles = "Administrator")]
    public class DeleteModel : PageModel
    {
        private readonly IDepartmentService _departmentService;

        public DeleteModel(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [BindProperty]
        public Department? Department { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Department = await _departmentService.GetDepartmentByIdAsync(id);
            if (Department == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Department == null)
            {
                return NotFound();
            }

            var deleted = await _departmentService.DeleteDepartmentAsync(Department.Id);
            if (!deleted)
            {
                TempData["Error"] = "Нельзя удалить отдел, в котором есть сотрудники.";
                return RedirectToPage("/Departments/Manage");
            }

            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && !referer.Contains("/Departments/Delete"))
            {
                return Redirect(referer);
            }

            return RedirectToPage("/Departments/Manage");
        }
    }
}


