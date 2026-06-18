using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using Blazored.LocalStorage;
using RMA.Client;
using RMA.Client.Auth;
using RMA.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddTransient<JwtAuthorizationHandler>();

// Register HttpClient using IHttpClientFactory with the JwtAuthorizationHandler message handler
builder.Services.AddHttpClient("RMA.ServerAPI", client => 
{
    client.BaseAddress = new Uri("http://localhost:5299");
})
.AddHttpMessageHandler<JwtAuthorizationHandler>();

// Override default HttpClient to ensure both direct injections and Services use the secured client
builder.Services.AddScoped(sp => 
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("RMA.ServerAPI"));

builder.Services.AddMudServices();

// Setup mock authorization
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, MockAuthStateProvider>();

builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<DeviceService>();
builder.Services.AddScoped<IRmaTicketService, RmaTicketService>();
builder.Services.AddScoped<ReferenceDataService>();

await builder.Build().RunAsync();
