using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Data.Models
{
    public class FinancialTransaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal Amount { get; set; }
        public string Type { get; set; } = "Expense"; // Income / Expense
        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? FamilyId { get; set; }           // null = личная транзакция
        public bool IsFamilyTransaction { get; set; } 

        public Family? Family { get; set; }

        public string TypeDisplay => Type == "Income" ? "Доход" : "Расход";
    }
}