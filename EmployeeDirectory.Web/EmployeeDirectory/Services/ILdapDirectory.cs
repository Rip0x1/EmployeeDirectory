using System.Threading.Tasks;

namespace EmployeeDirectory.Services
{
    public interface ILdapDirectory
    {
        Task<bool> IsEnabledAsync();
        Task<bool> UserExistsAsync(string userName);
        Task<bool> IsMemberOfAsync(string userName, string groupName);
        Task<IReadOnlyList<string>> GetDomainUserAccountsAsync();
        Task<IReadOnlyList<string>> GetDomainComputerAccountsAsync();
    }
}




