using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Product;

// Опис характеристики продукту, наприклад: склад, вікові обмеження, колір, класифікація тощо.
[Table("tblAttributeValues")]
public class AttributeValueEntity : BaseEntity<long>
{
    public long ProductAttributeId { get; set; }
    public ProductAttributeEntity ProductAttribute { get; set; } = null!;


    [Required, MaxLength(150)]
    public string Value { get; set; } = string.Empty;

    public ICollection<ProductAttributeValueEntity> ProductAttributeValues { get; set; } = [];
}
