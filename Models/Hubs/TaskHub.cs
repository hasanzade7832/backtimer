// File: backtimetracker/Hubs/TaskHub.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace backtimetracker.Hubs
{
    /// <summary>
    /// هاب SignalR برای تمام رویدادهای بلادرنگ مرتبط با «تسک‌ها».
    /// از Clients.User برای ارسال مستقیم به یک کاربر خاص استفاده می‌کنیم
    /// (UserIdentifier = ApplicationUser.Id در Identity).
    /// </summary>
    [Authorize]
    public class TaskHub : Hub
    {
        /*──────────────── پیام‌های مربوط به اختصاص و پیشرفت ────────────────*/

        /// <summary>
        /// اعلان «یک تسک جدید برایت ثبت شد».
        /// </summary>
        public async Task SendTaskAssignedMessage(string userId, int taskId)
            => await Clients.User(userId).SendAsync("TaskAssigned", taskId);

        /// <summary>
        /// اعلان «کاربر پیشرفت یا تکمیل تسک را گزارش کرد» برای ادمین سازندهٔ تسک.
        /// payload → { userTaskId, percent }
        /// </summary>
        public async Task SendTaskCompletedMessage(string adminId, object payload)
            => await Clients.User(adminId).SendAsync("TaskCompletedByUser", payload);

        /// <summary>
        /// اعلان «ادمین تسک را تأیید کرد» برای کاربر انجام‌دهنده.
        /// </summary>
        public async Task SendTaskConfirmedMessage(string userId, int userTaskId)
            => await Clients.User(userId).SendAsync("TaskConfirmed", userTaskId);

        /*──────────────── پیام‌های جدید: ویرایش یا حذف ─────────────────────*/

        /// <summary>
        /// اعلان «جزئیات تسک تغییر کرد» برای یک کاربر.
        /// </summary>
        public async Task SendTaskUpdatedMessage(string userId, int taskId)
            => await Clients.User(userId).SendAsync("TaskUpdated", taskId);

        /// <summary>
        /// اعلان «تسک از فهرست شما حذف شد» (به‌دلیل حذف یا لغو تخصیص).
        /// </summary>
        public async Task SendTaskDeletedMessage(string userId, int taskId)
            => await Clients.User(userId).SendAsync("TaskDeleted", taskId);

        /*──────────────── (اختیاری) رویداد اتصال ───────────────────────────

        // اگر بخواهید در زمان اتصال، کاربر را به گروه خاصی اضافه کنید
        // یا شناسهٔ اتصال را ذخیره کنید، می‌توانید این متد را فعال کنید.

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;   // ApplicationUser.Id
            // await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            await base.OnConnectedAsync();
        }
        */
    }
}
