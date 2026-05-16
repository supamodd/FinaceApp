using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Data.Models;

public class FamilyMember
{
    public int FamilyId { get; set; }
    public int UserId { get; set; }

    public FamilyRole Role { get; set; } = FamilyRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(FamilyId))]
    public Family Family { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}