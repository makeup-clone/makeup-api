using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Product;

namespace Domain.Entities;

[Table("tblBrands")]
public class BrandEntity : BaseEntity<long>
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<ProductEntity> Products { get; set; } = [];
}