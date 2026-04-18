using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Data.Models
{
    public class RecurringTransaction
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public decimal Amount { get; set; }

        public string Type { get; set; } = "Expense";        // "Income" или "Expense"

        public string Description { get; set; } = string.Empty;

        public string Frequency { get; set; } = "Monthly";   // Monthly, Weekly, BiWeekly, Yearly

        public int DayOfMonth { get; set; }                  // для ежемесячных — день месяца (1-31)

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime NextOccurrence { get; set; }
    }
}
