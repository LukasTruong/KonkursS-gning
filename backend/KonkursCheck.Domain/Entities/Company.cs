using KonkursCheck.Domain.Enums;

namespace KonkursCheck.Domain.Entities;

public class Company
{
    public string CvrNumber { get; set; } = default!;
    public string Name { get; set; } = default!;
    public CompanyStatus Status { get; set; } = CompanyStatus.Unknown;
    public DateOnly? FoundedDate { get; set; }
    public DateOnly? BankruptcyDate { get; set; }
    public DateOnly? DissolutionDate { get; set; }
    public string? IndustryCode { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public ICollection<PersonCompanyRole> PersonRoles { get; set; } = new List<PersonCompanyRole>();
}
