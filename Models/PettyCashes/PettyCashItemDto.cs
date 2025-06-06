using System;
using System.Collections.Generic;

namespace backtimetracker.Dtos.PettyCashes
{
    public class PettyCashItemDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public IEnumerable<ExpenseItemDto> Expenses { get; set; }
    }
}
