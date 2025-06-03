using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace backtimetracker.Hubs
{
    [Authorize]
    public class TaskHub : Hub
    {
        /// <summary>
        /// وقتی ادمین تسکی را اختصاص می‌دهد، برای کاربر ارسال می‌شود.
        /// </summary>
        public async Task SendTaskAssignedMessage(string userId, int taskId)
        {
            await Clients.User(userId).SendAsync("TaskAssigned", taskId);
        }

        /// <summary>
        /// وقتی کاربر یک تسک را تکمیل می‌کند، به ادمین اطلاع می‌دهد.
        /// </summary>
        public async Task SendTaskCompletedMessage(string adminId, int userTaskId)
        {
            await Clients.User(adminId).SendAsync("TaskCompletedByUser", userTaskId);
        }

        /// <summary>
        /// وقتی ادمین تسک تکمیل‌شده را تأیید می‌کند، به کاربر اطلاع می‌دهد.
        /// </summary>
        public async Task SendTaskConfirmedMessage(string userId, int userTaskId)
        {
            await Clients.User(userId).SendAsync("TaskConfirmed", userTaskId);
        }
    }
}
