using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EmployeeDirectory.Models;
using EmployeeDirectory.Services;
using System.ComponentModel.DataAnnotations;
using EmployeeDirectory.Data;

namespace EmployeeDirectory.Pages.Departments
{
    [Authorize(Roles = "Manager,Administrator")]
    public class AddEditorModel : PageModel
    {
        private readonly IDepartmentEditorService _editorService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILdapDirectory _ldapDirectory;

        public AddEditorModel(
            IDepartmentEditorService editorService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILdapDirectory ldapDirectory)
        {
            _editorService = editorService;
            _context = context;
            _userManager = userManager;
            _ldapDirectory = ldapDirectory;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty]
        public string DepartmentName { get; set; } = string.Empty;
        
        [BindProperty]
        public int DepartmentId { get; set; }

        public List<string> DomainAccounts { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Логин обязателен")]
            [Display(Name = "Логин")]
            public string UserName { get; set; } = string.Empty;

            [Display(Name = "Полное имя")]
            public string? FullName { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? departmentId)
        {
            if (departmentId == null || departmentId <= 0)
            {
                TempData["ErrorMessage"] = "Не указан ID отдела";
                return RedirectToPage("/Departments/Index");
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == departmentId);
            if (department == null)
            {
                TempData["ErrorMessage"] = $"Отдел с ID {departmentId} не найден в базе данных";
                return RedirectToPage("/Departments/Index");
            }

            DepartmentId = department.Id;
            DepartmentName = department.GetDisplayName();

            var accounts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var dbAccounts = _userManager.Users
                .Where(u => u.UserName != null && u.UserName.Contains("\\"))
                .Select(u => u.UserName!)
                .ToList();
            foreach (var a in dbAccounts) accounts.Add(a);

            if (await _ldapDirectory.IsEnabledAsync())
            {
                var adUsers = await _ldapDirectory.GetDomainUserAccountsAsync();
                var adComputers = await _ldapDirectory.GetDomainComputerAccountsAsync();
                foreach (var a in adUsers) accounts.Add(a);
                foreach (var c in adComputers) accounts.Add(c);
            }

            DomainAccounts = accounts.OrderBy(a => a).ToList();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (await _userManager.IsInRoleAsync(currentUser, "Administrator"))
            {
                return Page();
            }

            if (await _userManager.IsInRoleAsync(currentUser, "Manager") && currentUser.DepartmentId == departmentId)
            {
                return Page();
            }

            return Forbid();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var accounts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var dbAccounts = _userManager.Users
                    .Where(u => u.UserName != null && u.UserName.Contains("\\"))
                    .Select(u => u.UserName!)
                    .ToList();
                foreach (var a in dbAccounts) accounts.Add(a);

                if (await _ldapDirectory.IsEnabledAsync())
                {
                    var adUsers = await _ldapDirectory.GetDomainUserAccountsAsync();
                    var adComputers = await _ldapDirectory.GetDomainComputerAccountsAsync();
                    foreach (var a in adUsers) accounts.Add(a);
                    foreach (var c in adComputers) accounts.Add(c);
                }

                DomainAccounts = accounts.OrderBy(a => a).ToList();
                return Page();
            }

            try
            {
                var result = await _editorService.CreateDepartmentEditorAsync(
                    Input.UserName,
                    string.IsNullOrWhiteSpace(Input.FullName) ? string.Empty : Input.FullName,
                    DepartmentId);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Редактор отдела успешно создан";
                    return RedirectToPage("/Departments/Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка при создании редактора: {ex.Message}");
            }

            var accountsReload = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var dbAccountsReload = _userManager.Users
                .Where(u => u.UserName != null && u.UserName.Contains("\\"))
                .Select(u => u.UserName!)
                .ToList();
            foreach (var a in dbAccountsReload) accountsReload.Add(a);

            if (await _ldapDirectory.IsEnabledAsync())
            {
                var adUsers = await _ldapDirectory.GetDomainUserAccountsAsync();
                var adComputers = await _ldapDirectory.GetDomainComputerAccountsAsync();
                foreach (var a in adUsers) accountsReload.Add(a);
                foreach (var c in adComputers) accountsReload.Add(c);
            }

            DomainAccounts = accountsReload.OrderBy(a => a).ToList();
            return Page();
        }
    }
}
