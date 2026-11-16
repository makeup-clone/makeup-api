using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Identity;
using Domain.Entities.Product;

namespace Domain.Entities;

[Table("tblFavorites")]
public class FavoriteEntity : BaseEntity<long>
{
    public long ProductId { get; set; }
    public virtual ProductEntity? Product { get; set; }

    public long UserId { get; set; }
    public virtual UserEntity? User { get; set; }
}