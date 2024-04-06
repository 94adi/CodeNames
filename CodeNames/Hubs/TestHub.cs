using Microsoft.AspNetCore.SignalR;

namespace CodeNames.Hubs
{
    public class TestHub : Hub
    {

        public static int TotalOnlineUsers { get; set; } = 0;


        public override Task OnConnectedAsync()
        {
            //code here
            ++TotalOnlineUsers;
            Clients.All.SendAsync("updateTotalUsers", TotalOnlineUsers).GetAwaiter().GetResult();
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            //code here
            --TotalOnlineUsers;
            Clients.All.SendAsync("updateTotalUsers", TotalOnlineUsers).GetAwaiter().GetResult();
            return base.OnDisconnectedAsync(exception);
        }


    }
}
