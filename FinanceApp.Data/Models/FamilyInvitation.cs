using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Data.Models;

public class FamilyInvitation
{
    public int Id { get; set; }
    public int FamilyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public int CreatedByUserId { get; set; }

    public Family Family { get; set; } = null!;
}
