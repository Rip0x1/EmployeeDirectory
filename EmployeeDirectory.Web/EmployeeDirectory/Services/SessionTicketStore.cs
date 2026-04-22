using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;

namespace EmployeeDirectory.Services
{
    public class SessionTicketStore : ITicketStore
    {
        private const string TicketPrefix = "AuthTicket:";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionTicketStore(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var key = TicketPrefix + Guid.NewGuid().ToString();
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.Session != null)
            {
                var ticketData = SerializeTicket(ticket);
                httpContext.Session.SetString(key, ticketData);
            }

            return await Task.FromResult(key);
        }

        public async Task<AuthenticationTicket?> RetrieveAsync(string key)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.Session != null)
            {
                var ticketData = httpContext.Session.GetString(key);
                if (!string.IsNullOrEmpty(ticketData))
                {
                    return DeserializeTicket(ticketData);
                }
            }

            return await Task.FromResult<AuthenticationTicket?>(null);
        }

        public async Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.Session != null)
            {
                var ticketData = SerializeTicket(ticket);
                httpContext.Session.SetString(key, ticketData);
            }

            await Task.CompletedTask;
        }

        public async Task RemoveAsync(string key)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.Session != null)
            {
                httpContext.Session.Remove(key);
            }

            await Task.CompletedTask;
        }

        private string SerializeTicket(AuthenticationTicket ticket)
        {
            var ticketData = new
            {
                AuthenticationScheme = ticket.AuthenticationScheme,
                Principal = ticket.Principal.Claims.Select(c => new { c.Type, c.Value }).ToArray(),
                Properties = ticket.Properties.Items
            };

            return JsonSerializer.Serialize(ticketData);
        }

        private AuthenticationTicket? DeserializeTicket(string ticketData)
        {
            try
            {
                var ticketObj = JsonSerializer.Deserialize<JsonElement>(ticketData);
                var claims = ticketObj.GetProperty("Principal").EnumerateArray()
                    .Select(c => new System.Security.Claims.Claim(c.GetProperty("Type").GetString()!, c.GetProperty("Value").GetString()!))
                    .ToList();

                var identity = new System.Security.Claims.ClaimsIdentity(claims, ticketObj.GetProperty("AuthenticationScheme").GetString());
                var principal = new System.Security.Claims.ClaimsPrincipal(identity);

                var properties = new AuthenticationProperties();
                if (ticketObj.TryGetProperty("Properties", out var propsElement))
                {
                    foreach (var prop in propsElement.EnumerateObject())
                    {
                        properties.Items[prop.Name] = prop.Value.GetString();
                    }
                }

                return new AuthenticationTicket(principal, properties, ticketObj.GetProperty("AuthenticationScheme").GetString());
            }
            catch
            {
                return null;
            }
        }
    }
}

