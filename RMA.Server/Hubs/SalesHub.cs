using Microsoft.AspNetCore.SignalR;

namespace RMA.Server.Hubs
{
    public class SalesHub : Hub
    {
        // No client-to-server calls needed for status sync, just broadcast from controller.
    }
}
