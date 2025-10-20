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

        public EditModel(UserManager<ApplicationUser> userManager, IDepartmentService departmentService)
        {
            _userManager = userManager;
            _departmentService = departmentService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public SelectList Departments { get; set; } = default!;

        public class InputModel
        {
            public string Id { get; set; } = string.Empty;

            [Required(ErrorMessage = "Введите логин")]
            [Display(Name = "Логин")]
            public string UserName { get; set; } = string.Empty;

            [Display(Name = "Новый пароль")]
            public string NewPassword { get; set; } = string.Empty;

            [Display(Name = "Подтверждение пароля")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Display(Name = "Полное имя")]
            public string FullName { get; set; } = string.Empty;

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
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Input.NewPassword");
            ModelState.Remove("Input.ConfirmPassword");
            
            if (!string.IsNullOrEmpty(Input.NewPassword) && string.IsNullOrEmpty(Input.ConfirmPassword))
            {
                ModelState.AddModelError("Input.ConfirmPassword", "Подтвердите пароль");
            }
            
            if (!string.IsNullOrEmpty(Input.ConfirmPassword) && string.IsNullOrEmpty(Input.NewPassword))
            {
                ModelState.AddModelError("Input.NewPassword", "Введите новый пароль");
            }
            
            if (!string.IsNullOrEmpty(Input.NewPassword) && !string.IsNullOrEmpty(Input.ConfirmPassword) && Input.NewPassword != Input.ConfirmPassword)
            {
                ModelState.AddModelError("Input.ConfirmPassword", "Пароли не совпадают");
            }

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
            user.FullName = Input.FullName;
            user.DepartmentId = Input.DepartmentId;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(Input.NewPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, Input.NewPassword);
                }

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
