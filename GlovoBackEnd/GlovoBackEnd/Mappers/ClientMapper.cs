using GlovoBackEnd.Domain.Entities.Identity;
using GlovoBackEnd.Models.Dtos.Client;

namespace GlovoBackEnd.Mappers;

public static class ClientMapper
{
    public static ClientResponseDto ToClientResponseDto(this UserEntity user) =>
        new(user.Id, user.Email!, user.FirstName, user.LastName, user.Image, user.DateCreated);
}