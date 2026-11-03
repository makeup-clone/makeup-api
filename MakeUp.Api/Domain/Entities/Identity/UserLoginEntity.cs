using Microsoft.AspNetCore.Identity;


namespace Domain.Entities.Identity;

public class UserLoginEntity : IdentityUserLogin<long>
{
    public UserEntity User { get; set; }
}

