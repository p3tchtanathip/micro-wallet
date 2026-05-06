namespace Domain.Entities;

public class UserRole
{
    public long UserId { get; set; }
    public int RoleId { get; set; }

    // Relations
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}