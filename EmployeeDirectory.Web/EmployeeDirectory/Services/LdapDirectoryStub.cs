using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace EmployeeDirectory.Services
{
    public class LdapDirectoryStub : ILdapDirectory
    {
        private readonly IConfiguration _configuration;

        public LdapDirectoryStub(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<bool> IsEnabledAsync()
        {
            var enabled = _configuration.GetValue<bool>("Ldap:Enabled");
            return Task.FromResult(enabled);
        }

        public Task<bool> UserExistsAsync(string userName)
        {
            return Task.FromResult(true);
        }

        public Task<bool> IsMemberOfAsync(string userName, string groupName)
        {
            return Task.FromResult(false);
        }

        public Task<IReadOnlyList<string>> GetDomainUserAccountsAsync()
        {
            return Task.FromResult((IReadOnlyList<string>)new List<string>());
        }

        public Task<IReadOnlyList<string>> GetDomainComputerAccountsAsync()
        {
            return Task.FromResult((IReadOnlyList<string>)new List<string>());
        }
    }
}




