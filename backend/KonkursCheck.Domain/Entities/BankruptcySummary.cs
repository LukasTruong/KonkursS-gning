namespace KonkursCheck.Domain.Entities;

public class BankruptcySummary
{
    public string PersonCvrId { get; set; } = default!;
    public int TotalBankruptcies { get; set; }
    public DateOnly? MostRecentDate { get; set; }
    public string[] CompanyNames { get; set; } = Array.Empty<string>();
    public DateTime LastCalculated { get; set; } = DateTime.UtcNow;

    public Person Person { get; set; } = default!;
}
