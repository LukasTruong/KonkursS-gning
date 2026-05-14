using KonkursCheck.Domain.Entities;
using KonkursCheck.Domain.Enums;
using KonkursCheck.Infrastructure.Cvr;
using KonkursCheck.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KonkursCheck.Application.Services;

public class CvrIntegrationService
{
    private readonly AppDbContext _db;
    private readonly ICvrElasticClient _cvr;

    public CvrIntegrationService(AppDbContext db, ICvrElasticClient cvr)
    {
        _db = db;
        _cvr = cvr;
    }

    public async Task UpsertPersonAsync(string personId, string fullName, CancellationToken ct = default)
    {
        var person = await _db.Persons.FindAsync([personId], ct);
        if (person == null)
        {
            _db.Persons.Add(new Person { PersonCvrId = personId, FullName = fullName, LastUpdated = DateTime.UtcNow });
        }
        else
        {
            person.FullName = fullName;
            person.LastUpdated = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpsertCompanyAsync(CvrCompanyResult result, CancellationToken ct = default)
    {
        var company = await _db.Companies.FindAsync([result.CvrNumber], ct);
        var status = MapStatus(result.Status);

        if (company == null)
        {
            _db.Companies.Add(new Company
            {
                CvrNumber = result.CvrNumber,
                Name = result.Name,
                Status = status,
                FoundedDate = result.FoundedDate,
                BankruptcyDate = result.BankruptcyDate,
                IndustryCode = result.IndustryCode,
                LastUpdated = DateTime.UtcNow
            });
        }
        else
        {
            company.Name = result.Name;
            company.Status = status;
            company.FoundedDate = result.FoundedDate;
            company.BankruptcyDate = result.BankruptcyDate;
            company.IndustryCode = result.IndustryCode;
            company.LastUpdated = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpsertRoleAsync(string personId, string cvrNumber, string roleStr, DateOnly? start, DateOnly? end, CancellationToken ct = default)
    {
        var role = MapRole(roleStr);
        var existing = await _db.PersonCompanyRoles
            .FirstOrDefaultAsync(r => r.PersonCvrId == personId && r.CvrNumber == cvrNumber && r.Role == role && r.StartDate == start, ct);

        if (existing == null)
        {
            _db.PersonCompanyRoles.Add(new PersonCompanyRole
            {
                PersonCvrId = personId,
                CvrNumber = cvrNumber,
                Role = role,
                StartDate = start,
                EndDate = end
            });
            await _db.SaveChangesAsync(ct);
        }
        else
        {
            existing.EndDate = end;
            await _db.SaveChangesAsync(ct);
        }
    }

    public static CompanyStatus MapStatus(string status) => status.ToUpperInvariant() switch
    {
        var s when s.Contains("KONKURS") => CompanyStatus.Bankrupt,
        "AKTIV" => CompanyStatus.Active,
        var s when s.Contains("OPLOS") => CompanyStatus.Dissolved,
        _ => CompanyStatus.Unknown
    };

    public static RoleType MapRole(string role) => role.ToUpperInvariant() switch
    {
        var r when r.Contains("DIREKTØR") || r.Contains("DIREKTOER") || r.Contains("DIRECTOR") => RoleType.Director,
        var r when r.Contains("BESTYRELSE") || r.Contains("BOARD") => RoleType.BoardMember,
        var r when r.Contains("EJER") || r.Contains("OWNER") => RoleType.Owner,
        _ => RoleType.Other
    };
}
