using Application.Models;
using Domain.Entities.Identity;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public partial class UserMapper
{
    [MapperIgnoreTarget(nameof(UserEntity.Image))]
    [MapProperty(nameof(RegisterModel.Email), nameof(UserEntity.UserName))]
    public partial UserEntity RegisterModelToUserEntity(RegisterModel model);
}
