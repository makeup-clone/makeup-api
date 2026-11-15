using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Product;

[Table("tblProducts")]
public class ProductEntity : BaseEntity<long>
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;
    [Required, StringLength(255)]
    public string UrlSlug { get; set; } = string.Empty;

    public double AverageRating { get; set; }
    public int ReviewsCount { get; set; }

    public long BrandId { get; set; }
    public virtual BrandEntity Brand { get; set; } = null!;

    public virtual ICollection<ProductImageEntity> ProductImages { get; set; } = [];
    public virtual ICollection<ProductVariantEntity> Variants { get; set; } = [];
    public virtual ICollection<ReviewEntity> Reviews { get; set; } = [];
    public virtual ICollection<ProductAttributeValueEntity> AttributeValues { get; set; } = [];
}
