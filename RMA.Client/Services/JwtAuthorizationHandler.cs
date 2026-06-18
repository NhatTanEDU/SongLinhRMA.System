using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace RMA.Client.Services
{
    public class JwtAuthorizationHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly NavigationManager _navigationManager;

        public JwtAuthorizationHandler(ILocalStorageService localStorageService, NavigationManager navigationManager)
        {
            _localStorageService = localStorageService;
            _navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Đọc authToken từ Local Storage
            var token = await _localStorageService.GetItemAsync<string>("authToken", cancellationToken);

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            // Nếu nhận phản hồi 401 Unauthorized (ngoại trừ API đăng nhập chính), tự động chuyển hướng về trang đăng nhập
            if (response.StatusCode == HttpStatusCode.Unauthorized && 
                request.RequestUri != null && 
                !request.RequestUri.AbsolutePath.Contains("api/auth/login", StringComparison.OrdinalIgnoreCase))
            {
                _navigationManager.NavigateTo("/login");
            }

            return response;
        }
    }
}
