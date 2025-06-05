using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace backtimetracker.Hubs
{
    /// <summary>
    /// هاب SignalR برای ارتباط بی‌درنگ (real-time) پیام‌های مربوط به تسک.
    /// </summary>
    [Authorize]
    public class TaskHub : Hub
    {
        /// <summary>
        /// وقتی ادمین یک تسک را به کاربری اختصاص می‌دهد،
        /// این متد را از کنترلر فراخوانی می‌کنیم تا پیام «TaskAssigned» برای کاربر ارسال شود.
        /// </summary>
        /// <param name="userId">شناسهٔ کاربری که تسک برایش ارسال شده</param>
        /// <param name="taskId">شناسهٔ تسک اختصاص یافته</param>
        public async Task SendTaskAssignedMessage(string userId, int taskId)
        {
            await Clients.User(userId).SendAsync("TaskAssigned", taskId);
        }

        /// <summary>
        /// وقتی کاربر یک تسک را تکمیل می‌کند، از کنترلر
        /// فراخوانی می‌شود تا پیام «TaskCompletedByUser» برای ادمین ارسال شود.
        /// </summary>
        /// <param name="adminId">شناسهٔ ادمین سازندهٔ تسک</param>
        /// <param name="userTaskId">شناسهٔ رکورد UserTask (تسک–کاربر)</param>
        public async Task SendTaskCompletedMessage(string adminId, int userTaskId)
        {
            await Clients.User(adminId).SendAsync("TaskCompletedByUser", userTaskId);
        }

        /// <summary>
        /// وقتی ادمین یک تسک تکمیل‌شده توسط کاربر را تأیید می‌کند،
        /// این متد را فراخوانی می‌کنیم تا پیام «TaskConfirmed» برای کاربر ارسال شود.
        /// </summary>
        /// <param name="userId">شناسهٔ کاربری که تسک را انجام داده بود</param>
        /// <param name="userTaskId">شناسهٔ رکورد UserTask</param>
        public async Task SendTaskConfirmedMessage(string userId, int userTaskId)
        {
            await Clients.User(userId).SendAsync("TaskConfirmed", userTaskId);
        }

        /// <summary>
        /// (اختیاری) در صورت نیاز می‌توان متد زیر را Override کرد تا هنگام اتصال کاربر،
        /// او را به یک گروه خاص اضافه کنیم یا عملیات دیگری انجام دهیم.
        /// ولی در سناریوی کنونی که از Clients.User استفاده می‌شود، نیازی به این بخش نیست.
        /// </summary>
        // public override async Task OnConnectedAsync()
        // {
        //     var userId = Context.UserIdentifier;
        //     // اگر خواستید کاربران را در گروه‌هایی عضو کنید:
        //     // await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        //     await base.OnConnectedAsync();
        // }
    }
}
