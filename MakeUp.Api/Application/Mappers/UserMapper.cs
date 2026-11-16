using Application.Models;
using Domain.Entities.Identity;
using Google.Apis.Auth;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public partial class UserMapper
{
    [MapperIgnoreTarget(nameof(UserEntity.Image))]
    [MapProperty(nameof(RegisterModel.Email), nameof(UserEntity.UserName))]
    public partial UserEntity RegisterModelToUserEntity(RegisterModel model);

    [MapProperty(nameof(GoogleJsonWebSignature.Payload.GivenName), nameof(UserEntity.FirstName))]
    [MapProperty(nameof(GoogleJsonWebSignature.Payload.FamilyName), nameof(UserEntity.LastName))]
    [MapProperty(nameof(GoogleJsonWebSignature.Payload.Picture), nameof(UserEntity.Image))]
    [MapProperty(nameof(GoogleJsonWebSignature.Payload.Email), nameof(UserEntity.UserName))]
    public partial UserEntity GooglePayloadToUserEntity(GoogleJsonWebSignature.Payload payload);
}
