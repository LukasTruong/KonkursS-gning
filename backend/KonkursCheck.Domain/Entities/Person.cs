namespace KonkursCheck.Domain.Entities;

public class Person
{
    public string PersonCvrId { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public ICollection<PersonCompanyRole> Roles { get; set; } = new List<PersonCompanyRole>();
    public BankruptcySummary? BankruptcySummary { get; set; }
}
