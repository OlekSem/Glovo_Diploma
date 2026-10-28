namespace GlovoBackEnd.Mappers;

using GlovoBackEnd.Domain.Entities.Partner;
using GlovoBackEnd.Models.Dtos.Partner;

public static class PartnerMappers
{
    public static PartnerResponseDto ToPartnerResponseDto(this PartnerEntity partner)
    {
        return new PartnerResponseDto(
            partner.Id,
            partner.Email,
            partner.FirstName,
            partner.LastName,
            partner.Phone,
            partner.PartnerRoles.Select(x => x.Role.Name).ToArray(),
            partner.DateCreated
        );
    }
}
