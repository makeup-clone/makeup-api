using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Identity;
using Domain.Entities.Product;

namespace Domain.Entities;

[Table("tblCarts")]
public class CartEntity : BaseEntity<long>
{
    public long UserId { get; set; }
    public virtual UserEntity User { get; set; } = null!;

    public long ProductId { get; set; }
    public virtual ProductEntity Product { get; set; } = null!;

    [Range(0, 99)]
    public int Quantity { get; set; }
}


