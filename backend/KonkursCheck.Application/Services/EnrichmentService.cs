using KonkursCheck.Domain.Entities;
using KonkursCheck.Domain.Enums;
using KonkursCheck.Infrastructure.Caching;
using KonkursCheck.Infrastructure.Cvr;
using KonkursCheck.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KonkursCheck.Application.Services;

public class EnrichmentService
{
    private readonly AppDbContext _db;
    private readonly ICvrElasticClient _cvr;
    private readonly IRedisCacheService _cache;
    private readonly CvrIntegrationService _integration;

    public EnrichmentService(AppDbContext db, ICvrElasticClient cvr, IRedisCacheService cache, CvrIntegrationService integration)
    {
        _db = db;
        _cvr = cvr;
        _cache = cache;
        _integration = integration;
    }

    public async Task EnrichPersonAsync(string personId, CancellationToken ct = default)
    {
        var bankruptcies = await _cvr.GetPersonBankruptciesAsync(personId, ct);
        foreach (var company in bankruptcies)
        {
            await _integration.UpsertCompanyAsync(company, ct);

            var persons = await _cvr.GetCompanyPersonsAsync(company.CvrNumber, ct);
            foreach (var p in persons.Where(p => p.PersonId == personId))
            {
                await _integration.UpsertRoleAsync(personId, company.CvrNumber, p.Role, p.StartDate, p.EndDate, ct);
            }
        }
        await RecalculateBankruptcySummaryAsync(personId, ct);
    }

    public async Task RecalculateBankruptcySummaryAsync(string personId, CancellationToken ct = default)
    {
        var bankruptCompanies = await _db.PersonCompanyRoles
            .Include(r => r.Company)
            .Where(r => r.PersonCvrId == personId && r.Company.Status == CompanyStatus.Bankrupt)
            .Select(r => r.Company)
            .Distinct()
            .ToListAsync(ct);

        var summary = await _db.BankruptcySummaries.FindAsync([personId], ct);
        var mostRecent = bankruptCompanies
            .Where(c => c.BankruptcyDate.HasValue)
            .MaxBy(c => c.BankruptcyDate)?.BankruptcyDate;

        if (summary == null)
        {
            _db.BankruptcySummaries.Add(new BankruptcySummary
            {
                PersonCvrId = personId,
                TotalBankruptcies = bankruptCompanies.Count,
                MostRecentDate = mostRecent,
                CompanyNames = bankruptCompanies.Select(c => c.Name).ToArray(),
                LastCalculated = DateTime.UtcNow
            });
        }
        else
        {
            summary.TotalBankruptcies = bankruptCompanies.Count;
            summary.MostRecentDate = mostRecent;
            summary.CompanyNames = bankruptCompanies.Select(c => c.Name).ToArray();
            summary.LastCalculated = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CacheKeys.Person(personId));
        await _cache.RemoveAsync(CacheKeys.PersonBankruptcies(personId));
    }
}
