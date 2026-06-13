using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using RMA.Shared.DTOs; // Or a specific UserDto if you have one.

namespace RMA.Client.Auth
{
    public class MockAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        public void MarkUserAsAuthenticated(string username)
        {
            var role = "Admin";
            if (username.StartsWith("sales", StringComparison.OrdinalIgnoreCase))
            {
                role = "Sales";
            }
            else if (username.StartsWith("tech", StringComparison.OrdinalIgnoreCase))
            {
                role = "Tech";
            }

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            }, "MockAuthenticationType");

            _currentUser = new ClaimsPrincipal(identity);
            
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void MarkUserAsLoggedOut()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
