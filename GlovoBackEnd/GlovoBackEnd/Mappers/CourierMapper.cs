using GlovoBackEnd.Domain.Entities.Identity;
using GlovoBackEnd.Models.Dtos.Courier;

namespace GlovoBackEnd.Mappers;

public static class CourierMapper
{
    public static CourierResponseDto ToCourierResponseDto(this UserEntity user) =>
        new(user.Id, user.Email!, user.FirstName, user.LastName, user.Image, user.DateCreated);
}