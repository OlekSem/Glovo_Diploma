namespace GlovoBackEnd.Mappers;

using GlovoBackEnd.Domain.Entities.Identity;
using GlovoBackEnd.Models.Dtos.Partner;

public static class PartnerMappers
{
    public static PartnerResponseDto ToPartnerResponseDto(this UserEntity user)
    {
        return new PartnerResponseDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Image,
            user.DateCreated
        );
    }
}