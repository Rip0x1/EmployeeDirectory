using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using EmployeeDirectory.Models;
using EmployeeDirectory.Services;
using System.ComponentModel.DataAnnotations;

namespace EmployeeDirectory.Pages.Users
{
    [Authorize(Roles = "Administrator")]
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDepartmentService _departmentService;
        private readonly ILdapDirectory _ldapDirectory;

        public EditModel(UserManager<ApplicationUser> userManager, IDepartmentService departmentService, ILdapDirectory ldapDirectory)
        {
            _userManager = userManager;
            _departmentService = departmentService;
            _ldapDirectory = ldapDirectory;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public SelectList Departments { get; set; } = default!;
        public List<string> DomainAccounts { get; set; } = new();

        public class InputModel
        {
            public string Id { get; set; } = string.Empty;

            [Required(ErrorMessage = "Введите логин")]
            [Display(Name = "Логин")]
            public string UserName { get; set; } = string.Empty;

            

            [Display(Name = "Полное имя")]
            public string? FullName { get; set; }

            [Display(Name = "Отдел")]
            public int? DepartmentId { get; set; }

            [Required(ErrorMessage = "Выберите роль")]
            [Display(Name = "Роль")]
            public string Role { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var departments = await _departmentService.GetAllDepartmentsAsync();

            Input = new InputModel
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                FullName = user.FullName,
                DepartmentId = user.DepartmentId,
                Role = roles.FirstOrDefault() ?? ""
            };

            Departments = new SelectList(departments, "Id", "Name");

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

        public async Task<IActionResult> OnPostAsync()
        {
            

            if (!ModelState.IsValid)
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();
                Departments = new SelectList(departments, "Id", "Name");
                return Page();
            }

            var user = await _userManager.FindByIdAsync(Input.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = Input.UserName;
            user.FullName = string.IsNullOrWhiteSpace(Input.FullName) ? string.Empty : Input.FullName;
            user.DepartmentId = Input.DepartmentId;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {       
                

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                
                if (!string.IsNullOrEmpty(Input.Role))
                {
                    await _userManager.AddToRoleAsync(user, Input.Role);
                }

                TempData["Success"] = $"Пользователь {Input.UserName} успешно обновлен!";
                
                if (!string.IsNullOrEmpty(ReturnUrl))
                {
                    return Redirect(ReturnUrl);
                }

                var referer = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer) && !referer.Contains("/Users/Edit"))
                {
                    return Redirect(referer);
                }

                return RedirectToPage("/Users/Manage");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var departmentsReload = await _departmentService.GetAllDepartmentsAsync();
            Departments = new SelectList(departmentsReload, "Id", "Name");
            return Page();
        }
    }
}
