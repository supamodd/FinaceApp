using FinanceApp.Data;
using FinanceApp.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Services;

public class FamilyService
{
    private readonly AppDbContext _context;

    public FamilyService(AppDbContext context)
    {
        _context = context;
    }

    // === СОЗДАТЬ СЕМЬЮ ===
    public async Task<Family> CreateFamilyAsync(int userId, string familyName)
    {
        // Проверяем, существует ли пользователь
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new Exception("Пользователь не найден в базе");
        }

        var family = new Family
        {
            Name = familyName.Trim(),
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var member = new FamilyMember
        {
            FamilyId = family.Id,
            UserId = userId,
            Role = FamilyRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        _context.FamilyMembers.Add(member);
        await _context.SaveChangesAsync();

        return family;
    }

    // === ПОЛУЧИТЬ СЕМЬИ ПОЛЬЗОВАТЕЛЯ ===
    public async Task<List<Family>> GetUserFamiliesAsync(int userId)
    {
        return await _context.FamilyMembers
            .Where(fm => fm.UserId == userId)
            .Include(fm => fm.Family)
            .Select(fm => fm.Family)
            .ToListAsync();
    }

    // === СГЕНЕРИРОВАТЬ КОД ПРИГЛАШЕНИЯ ===
    public async Task<string> GenerateInvitationCodeAsync(int familyId, int createdByUserId)
    {
        var code = Guid.NewGuid().ToString("N")[..8].ToUpper();

        var invitation = new FamilyInvitation
        {
            FamilyId = familyId,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByUserId = createdByUserId,
            IsUsed = false
        };

        _context.FamilyInvitations.Add(invitation);
        await _context.SaveChangesAsync();

        return code;
    }

    // === ПРИСОЕДИНИТЬСЯ ПО КОДУ ===
    public async Task<bool> JoinByInvitationCodeAsync(int userId, string code)
    {
        var invitation = await _context.FamilyInvitations
            .FirstOrDefaultAsync(i => i.Code == code.ToUpper() &&
                                      !i.IsUsed &&
                                      i.ExpiresAt > DateTime.UtcNow);

        if (invitation == null)
            return false;

        // Проверяем, не состоит ли уже пользователь в этой семье
        var alreadyMember = await _context.FamilyMembers
            .AnyAsync(fm => fm.FamilyId == invitation.FamilyId && fm.UserId == userId);

        if (alreadyMember)
            return false;

        var member = new FamilyMember
        {
            FamilyId = invitation.FamilyId,
            UserId = userId,
            Role = FamilyRole.Member,
            JoinedAt = DateTime.UtcNow
        };

        _context.FamilyMembers.Add(member);
        invitation.IsUsed = true;

        await _context.SaveChangesAsync();
        return true;
    }

    // === ПОЛУЧИТЬ ЧЛЕНОВ СЕМЬИ ===
    public async Task<List<FamilyMember>> GetFamilyMembersAsync(int familyId)
    {
        return await _context.FamilyMembers
            .Where(fm => fm.FamilyId == familyId)
            .Include(fm => fm.User)
            .OrderByDescending(fm => fm.Role)
            .ToListAsync();
    }

    // === СМЕНИТЬ РОЛЬ ===
    public async Task<bool> ChangeRoleAsync(int familyId, int targetUserId, FamilyRole newRole, int currentUserId)
    {
        var currentMember = await _context.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.FamilyId == familyId && fm.UserId == currentUserId);

        if (currentMember == null || currentMember.Role > FamilyRole.Admin)
            return false; // Только Owner и Admin могут менять роли

        var targetMember = await _context.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.FamilyId == familyId && fm.UserId == targetUserId);

        if (targetMember == null)
            return false;

        // Owner не может понизить другого Owner
        if (targetMember.Role == FamilyRole.Owner && newRole != FamilyRole.Owner)
            return false;

        targetMember.Role = newRole;
        await _context.SaveChangesAsync();
        return true;
    }

    // === ПОКИНУТЬ СЕМЬЮ ===
    public async Task<bool> LeaveFamilyAsync(int familyId, int userId)
    {
        var member = await _context.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.FamilyId == familyId && fm.UserId == userId);

        if (member == null)
            return false;

        if (member.Role == FamilyRole.Owner)
            return false; // Owner не может просто уйти

        _context.FamilyMembers.Remove(member);
        await _context.SaveChangesAsync();
        return true;
    }

    // === ПОЛУЧИТЬ СЕМЬЮ ПО ID ===
    public async Task<Family?> GetFamilyByIdAsync(int familyId)
    {
        return await _context.Families
            .Include(f => f.Members)
            .FirstOrDefaultAsync(f => f.Id == familyId);
    }

    public async Task<bool> RemoveMemberAsync(int familyId, int targetUserId, int currentUserId)
    {
        var currentMember = await _context.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.FamilyId == familyId && fm.UserId == currentUserId);

        if (currentMember == null || currentMember.Role > FamilyRole.Admin)
            return false; // Только Owner и Admin

        var targetMember = await _context.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.FamilyId == familyId && fm.UserId == targetUserId);

        if (targetMember == null || targetMember.Role == FamilyRole.Owner)
            return false; // Нельзя исключить Owner

        _context.FamilyMembers.Remove(targetMember);
        await _context.SaveChangesAsync();
        return true;
    }
}