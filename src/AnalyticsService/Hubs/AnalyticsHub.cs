using Microsoft.AspNetCore.SignalR;

namespace AnalyticsService.Hubs;

public class AnalyticsHub : Hub
{
    public async Task SendPageView(string page) =>
        await Clients.All.SendAsync("PageView", new { page, timestamp = DateTime.UtcNow });
}