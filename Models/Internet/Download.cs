using System;

namespace backtimetracker.Models.Internet
{
    public class Download
    {
        public int Id { get; set; }

        public int PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }

        public int Volume { get; set; }

        public string? Desc { get; set; }

        public string? Date { get; set; }
    }
}
