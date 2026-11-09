namespace Infrastructure.Seed.Models;

public class UserSeedModel
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Image { get; set; }
    public List<string> Roles { get; set; } = new();
}
