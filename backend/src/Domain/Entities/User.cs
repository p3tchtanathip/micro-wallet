using Domain.Enums;

namespace Domain.Entities;

public class User
{
	public long Id { get; set; }
	public required string Email { get; set; }
	public required string Password { get; set; }
	public string? FullName { get; set; }
	public AuthProvider Provider { get; set; } = AuthProvider.Local;
	public bool IsActive { get; set; } = true;
	public DateTime CreatedAt { get; set; }
	public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

	// Relations
	public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
	public virtual ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
}
