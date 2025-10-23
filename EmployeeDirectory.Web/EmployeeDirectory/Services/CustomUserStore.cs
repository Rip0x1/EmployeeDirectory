using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EmployeeDirectory.Data;
using EmployeeDirectory.Models;
using System.Security.Claims;

namespace EmployeeDirectory.Services
{
    public class CustomUserStore : UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, string>
    {
        public CustomUserStore(ApplicationDbContext context, IdentityErrorDescriber? describer = null)
            : base(context, describer)
        {
        }

        public override async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            // Возвращаем пустой список claims, так как мы не используем таблицу AspNetUserClaims
            return new List<Claim>();
        }

        public override async Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            // Не добавляем claims, так как мы не используем таблицу AspNetUserClaims
        }

        public override async Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            // Не удаляем claims, так как мы не используем таблицу AspNetUserClaims
        }

        public override async Task ReplaceClaimAsync(ApplicationUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default)
        {
            // Не заменяем claims, так как мы не используем таблицу AspNetUserClaims
        }

        public override async Task<IList<ApplicationUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
        {
            // Возвращаем пустой список, так как мы не используем таблицу AspNetUserClaims
            return new List<ApplicationUser>();
        }

        public override async Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            // Возвращаем пустой список, так как мы не используем таблицу AspNetUserLogins
            return new List<UserLoginInfo>();
        }

        public override async Task AddLoginAsync(ApplicationUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
        {
            // Не добавляем логины, так как мы не используем таблицу AspNetUserLogins
        }

        public override async Task RemoveLoginAsync(ApplicationUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            // Не удаляем логины, так как мы не используем таблицу AspNetUserLogins
        }

        public override async Task<ApplicationUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            // Не ищем по логину, так как мы не используем таблицу AspNetUserLogins
            return null;
        }

        public override async Task SetTokenAsync(ApplicationUser user, string loginProvider, string name, string? value, CancellationToken cancellationToken = default)
        {
            // Не устанавливаем токены, так как мы не используем таблицу AspNetUserTokens
        }

        public override async Task RemoveTokenAsync(ApplicationUser user, string loginProvider, string name, CancellationToken cancellationToken = default)
        {
            // Не удаляем токены, так как мы не используем таблицу AspNetUserTokens
        }

        public override async Task<string?> GetTokenAsync(ApplicationUser user, string loginProvider, string name, CancellationToken cancellationToken = default)
        {
            // Не возвращаем токены, так как мы не используем таблицу AspNetUserTokens
            return null;
        }
    }
}
