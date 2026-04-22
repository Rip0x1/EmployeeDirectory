using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using EmployeeDirectory.Models;
using EmployeeDirectory.Services;
using Microsoft.AspNetCore.Mvc;
using EmployeeDirectory.Data;

namespace EmployeeDirectory.Pages.Departments
{
    [Authorize(Roles = "Manager,Administrator")]
    public class ManageEditorsModel : PageModel
    {
        private readonly IDepartmentEditorService _editorService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManageEditorsModel(
            IDepartmentEditorService editorService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _editorService = editorService;
            _context = context;
            _userManager = userManager;
        }

        public List<ApplicationUser> Editors { get; set; } = new();
        public string DepartmentName { get; set; } = string.Empty;
        public int DepartmentId { get; set; }

        public async Task<IActionResult> OnGetAsync(int? departmentId)
        {
            if (departmentId == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null)
            {
                return NotFound();
            }

            DepartmentId = department.Id;
            DepartmentName = department.GetDisplayName();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (await _userManager.IsInRoleAsync(currentUser, "Administrator"))
            {
                Editors = await _editorService.GetDepartmentEditorsAsync(departmentId.Value);
                return Page();
            }

            if (await _userManager.IsInRoleAsync(currentUser, "Manager") && currentUser.DepartmentId == departmentId)
            {
                Editors = await _editorService.GetDepartmentEditorsAsync(departmentId.Value);
                return Page();
            }

            return Forbid();
        }
    }
}
