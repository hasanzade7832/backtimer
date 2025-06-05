// Models/Task/UserTask.cs
using backtimetracker.Models.Task;
using backtimetracker.Models.User;

public class UserTask
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public bool IsCompletedByUser { get; set; }
    public bool IsConfirmedByAdmin { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    public bool IsSeenByUser { get; set; }
    public bool IsSeenByAdmin { get; set; }

    /* --- ستون جدید --- */
    public int PercentComplete { get; set; } = 0;
}
