using KonkursCheck.Domain.Enums;

namespace KonkursCheck.Domain.Entities;

public class PersonCompanyRole
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PersonCvrId { get; set; } = default!;
    public string CvrNumber { get; set; } = default!;
    public RoleType Role { get; set; } = RoleType.Other;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public Person Person { get; set; } = default!;
    public Company Company { get; set; } = default!;
}
