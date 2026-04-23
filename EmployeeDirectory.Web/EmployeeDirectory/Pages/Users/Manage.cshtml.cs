using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDirectory.Pages.Users
{
    [Authorize(Roles = "Administrator")]
    public class ManageModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public ManageModel(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IList<UserViewModel> Users { get; set; } = new List<UserViewModel>();

        public class UserViewModel
        {
            public string Id { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public Department? Department { get; set; }
            public List<string> Roles { get; set; } = new List<string>();
        }

        public async Task OnGetAsync()
        {
            var users = await _userManager.Users
                .Include(u => u.Department)
                .ToListAsync();

            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    FullName = user.FullName,
                    Department = user.Department,
                    Roles = roles.ToList()
                });
            }

            Users = userViewModels;
        }
    }
}
