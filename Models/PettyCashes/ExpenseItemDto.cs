namespace backtimetracker.Dtos.PettyCashes
{
    public class ExpenseItemDto
    {
        public long Id { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public long Amount { get; set; }
        public string ReceiptUrl { get; set; }
    }
}
