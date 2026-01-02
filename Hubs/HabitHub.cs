using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace HabitTracker.Hubs
{
    public class HabitHub : Hub
    {
        // Method to notify all clients about habit updates
        public async Task NotifyHabitUpdate(string userId, string message, int habitId = 0)
        {
            // Send notification to specific user
            await Clients.User(userId).SendAsync("ReceiveHabitUpdate", message, habitId);
        }

        // Method to notify about progress changes
        public async Task NotifyProgressUpdate(string userId, int habitId, int progress)
        {
            await Clients.User(userId).SendAsync("ReceiveProgressUpdate", habitId, progress);
        }

        // Method to notify when habit is completed
        public async Task NotifyHabitCompleted(string userId, int habitId, string habitName)
        {
            await Clients.User(userId).SendAsync("ReceiveHabitCompleted", habitId, habitName);
        }

        // Method to join user-specific group
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        // Method to leave user-specific group
        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        // Override OnConnectedAsync to automatically join user group
        public override async Task OnConnectedAsync()
        {
            // Get user ID from claims if available
            var userId = Context.User?.FindFirst("sub")?.Value ??
                        Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }

            await base.OnConnectedAsync();
        }

        // Override OnDisconnectedAsync to clean up groups
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Get user ID from claims if available
            var userId = Context.User?.FindFirst("sub")?.Value ??
                        Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
