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
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDepartmentService _departmentService;
        private readonly ILdapDirectory _ldapDirectory;

        public CreateModel(UserManager<ApplicationUser> userManager, IDepartmentService departmentService, ILdapDirectory ldapDirectory)
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

        public async Task OnGetAsync()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
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
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input.Role == "DepartmentEditor" && !Input.DepartmentId.HasValue)
            {
                ModelState.AddModelError("Input.DepartmentId", "Для редактора отдела необходимо выбрать отдел");
            }

            if (!ModelState.IsValid)
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();
                Departments = new SelectList(departments, "Id", "Name");
                return Page();
            }

            var user = new ApplicationUser
            {
                UserName = Input.UserName,
                FullName = string.IsNullOrWhiteSpace(Input.FullName) ? string.Empty : Input.FullName,
                DepartmentId = Input.DepartmentId
            };

            var result = await _userManager.CreateAsync(user);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(Input.Role))
                {
                    await _userManager.AddToRoleAsync(user, Input.Role);
                }

                TempData["Success"] = $"Пользователь {Input.UserName} успешно создан!";
                
                if (!string.IsNullOrEmpty(ReturnUrl))
                {
                    return Redirect(ReturnUrl);
                }

                var referer = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer) && !referer.Contains("/Users/Create"))
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
