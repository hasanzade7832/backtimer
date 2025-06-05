// backtimetracker/Models/Purchase.cs
using System;
using System.Collections.Generic;

namespace backtimetracker.Models.Internet
{
    public class Purchase
    {
        public int Id { get; set; }
        public string? UserId { get; set; }

        public int Amount { get; set; }

        public int TotalVolume { get; set; }

        public int RemainingVolume { get; set; }

        public string? Date { get; set; }

        public List<Download> Downloads { get; set; } = new List<Download>();
    }
}
