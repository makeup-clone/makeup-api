using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Identity;

public class UserEntity : IdentityUser<long>
{
    public DateTime DateCreated { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
    [StringLength(100)]
    public string? FirstName { get; set; } = null;
    [StringLength(100)]
    public string? LastName { get; set; } = null;
    [StringLength(100)]
    public string? Image { get; set; } = null;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();
    public virtual ICollection<UserRoleEntity>? UserRoles { get; set; }
    public virtual ICollection<UserLoginEntity>? UserLogins { get; set; }
    public virtual ICollection<ReviewEntity>? Reviews { get; set; }
    public virtual ICollection<CartEntity>? Carts { get; set; }
    public virtual ICollection<FavoriteEntity>? Favorites { get; set; }

    public bool IsDeleted { get; set; }
}
