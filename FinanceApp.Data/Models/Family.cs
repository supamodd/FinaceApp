using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Data.Models;

public class Family
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навигационные свойства
    public User CreatedBy { get; set; } = null!;
    public ICollection<FamilyMember> Members { get; set; } = new List<FamilyMember>();
    public ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}