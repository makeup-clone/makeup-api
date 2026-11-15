using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Identity;
using Domain.Entities.Product;

namespace Domain.Entities;

[Table("tblReviews")]
public class ReviewEntity : BaseEntity<long>
{
    public long? UserId { get; set; }
    public UserEntity? User { get; set; }

    [Required, MaxLength(250)]
    public string AuthorName { get; set; } = string.Empty;

    public long ProductId { get; set; }
    public ProductEntity? Product { get; set; }

    [Range(1, 5)]

    public int? Rating { get; set; }
    public string Text { get; set; } = string.Empty;

    public int LikesCount { get; set; }
    public int DislikesCount { get; set; }

    public long? ParentId { get; set; }
    public ReviewEntity? Parent { get; set; }
    public ICollection<ReviewEntity> Replies { get; set; } = [];
}