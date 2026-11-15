using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Product;
//Назва характеристики
[Table("tblProductAttributes")]
public class ProductAttributeEntity : BaseEntity<long>
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty; 
    public string Slug { get; set; } = string.Empty;

    public ICollection<AttributeValueEntity> Values { get; set; } = [];
}