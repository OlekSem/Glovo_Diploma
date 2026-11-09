namespace GlovoBackEnd.Models.Address;

public sealed record ClientAddressDto(
    string Region,
    string Locality
);