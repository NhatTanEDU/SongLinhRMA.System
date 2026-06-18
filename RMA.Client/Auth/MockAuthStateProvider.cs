using System;
using System.Security.Claims;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using RMA.Shared.DTOs;

namespace RMA.Client.Auth
{
    public class MockAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ISyncLocalStorageService _localStorage;
        private readonly HttpClient _httpClient;
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        public MockAuthStateProvider(ISyncLocalStorageService localStorage, HttpClient httpClient)
        {
            _localStorage = localStorage;
            _httpClient = httpClient;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_currentUser.Identity == null || !_currentUser.Identity.IsAuthenticated)
            {
                try
                {
                    var username = _localStorage.GetItem<string>("authTokenUsername");
                    var token = _localStorage.GetItem<string>("authToken");
                    var role = _localStorage.GetItem<string>("authTokenRole");

                    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(token))
                    {
                        if (string.IsNullOrEmpty(role))
                        {
                            role = "Admin";
                            if (username.StartsWith("sales", StringComparison.OrdinalIgnoreCase))
                            {
                                role = "Sales";
                            }
                            else if (username.StartsWith("tech", StringComparison.OrdinalIgnoreCase))
                            {
                                role = "Tech";
                            }
                        }

                        var identity = new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.Name, username),
                            new Claim(ClaimTypes.Role, role)
                        }, "MockAuthenticationType");

                        _currentUser = new ClaimsPrincipal(identity);
                        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    }
                }
                catch
                {
                    // Fallback during initialization
                }
            }

            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        public void MarkUserAsAuthenticated(string username, string token = "", string role = "")
        {
            if (!string.IsNullOrEmpty(token))
            {
                _localStorage.SetItem("authTokenUsername", username);
                _localStorage.SetItem("authToken", token);
                if (!string.IsNullOrEmpty(role))
                {
                    _localStorage.SetItem("authTokenRole", role);
                }
            }

            if (string.IsNullOrEmpty(role))
            {
                role = "Admin";
                if (username.StartsWith("sales", StringComparison.OrdinalIgnoreCase))
                {
                    role = "Sales";
                }
                else if (username.StartsWith("tech", StringComparison.OrdinalIgnoreCase))
                {
                    role = "Tech";
                }
            }

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            }, "MockAuthenticationType");

            _currentUser = new ClaimsPrincipal(identity);
            
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void MarkUserAsLoggedOut()
        {
            try
            {
                _localStorage.RemoveItem("authTokenUsername");
                _localStorage.RemoveItem("authToken");
                _localStorage.RemoveItem("authTokenRole");
            }
            catch
            {
                // Fallback
            }

            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            _httpClient.DefaultRequestHeaders.Authorization = null;
            
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
