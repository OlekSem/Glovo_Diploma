namespace GlovoBackEnd.Domain.Entities.Partner;

public static class PartnerRoleNames
{
    public const string CompanyOwner = "CompanyOwner";
    public const string AffiliateManager = "AffiliateManager";
    public const string AffiliateEmployee = "AffiliateEmployee";
    public const string User = "User";
    public const string AnyPartnerRole = CompanyOwner + "," + AffiliateManager + "," + AffiliateEmployee + "," + User;
}
