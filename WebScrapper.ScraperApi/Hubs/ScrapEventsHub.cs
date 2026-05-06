using Microsoft.AspNetCore.SignalR;

namespace WebScrapper.ScraperApi.Hubs;

public class ScrapEventsHub : Hub
{
    public Task JoinRun(string streamId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, streamId);
    }

    public Task LeaveRun(string streamId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, streamId);
    }
}
