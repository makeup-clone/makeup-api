namespace Infrastructure.Seed.Models;
public class ReviewSeedModel
{
    public long ProductId { get; set; }
    public long UserId { get; set; }
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public int LikesCount { get; set; }
    public int DislikesCount { get; set; }
    public long? ParentId { get; set; }
    public ICollection<ReviewSeedModel> Replies { get; set; } = [];
}