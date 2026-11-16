using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Product;
//Прив'язка характеристики до продукту
[Table("tblProductAttributeValues")]
public class ProductAttributeValueEntity
{
    public long ProductId { get; set; }
    public ProductEntity Product { get; set; } = null!;

    public long AttributeValueId { get; set; }
    public AttributeValueEntity AttributeValue { get; set; } = null!;
}
