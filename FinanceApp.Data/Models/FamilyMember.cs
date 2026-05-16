using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Data.Models;

public class FamilyMember
{
    public int FamilyId { get; set; }
    public int UserId { get; set; }

    public FamilyRole Role { get; set; } = FamilyRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Навигация
    public Family Family { get; set; } = null!;
    public User User { get; set; } = null!;
}

public enum FamilyRole
{
    Owner = 0,      // Владелец (может удалять семью и менять роли)
    Admin = 1,      // Администратор
    Member = 2,     // Обычный участник
    Viewer = 3      // Только просмотр
}
