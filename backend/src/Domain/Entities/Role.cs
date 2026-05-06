namespace Domain.Entities;

public class Role
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Relations
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}