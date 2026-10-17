namespace GlovoBackEnd.Models.Dtos.Common;

public sealed record AuthTokensDto
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}