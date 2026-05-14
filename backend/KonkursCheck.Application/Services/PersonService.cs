using KonkursCheck.Application.DTOs;
using KonkursCheck.Domain.Enums;
using KonkursCheck.Infrastructure.Caching;
using KonkursCheck.Infrastructure.Cvr;
using KonkursCheck.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KonkursCheck.Application.Services;

public class PersonService
{
    private readonly AppDbContext _db;
    private readonly ICvrElasticClient _cvr;
    private readonly IRedisCacheService _cache;
    private readonly EnrichmentService _enrichment;

    public PersonService(AppDbContext db, ICvrElasticClient cvr, IRedisCacheService cache, EnrichmentService enrichment)
    {
        _db = db;
        _cvr = cvr;
        _cache = cache;
        _enrichment = enrichment;
    }

    public async Task<PersonProfileDto?> GetProfileAsync(string personId, CancellationToken ct = default)
    {
        var cacheKey = CacheKeys.Person(personId);
        var cached = await _cache.GetAsync<PersonProfileDto>(cacheKey);
        if (cached != null) return cached;

        var person = await _db.Persons
            .Include(p => p.Roles).ThenInclude(r => r.Company)
            .Include(p => p.BankruptcySummary)
            .FirstOrDefaultAsync(p => p.PersonCvrId == personId, ct);

        if (person == null)
        {
            await _enrichment.EnrichPersonAsync(personId, ct);
            person = await _db.Persons
                .Include(p => p.Roles).ThenInclude(r => r.Company)
                .Include(p => p.BankruptcySummary)
                .FirstOrDefaultAsync(p => p.PersonCvrId == personId, ct);
        }

        if (person == null) return null;

        var dto = new PersonProfileDto(
            person.PersonCvrId,
            person.FullName,
            person.BankruptcySummary?.TotalBankruptcies ?? 0,
            person.Roles.Select(r => new RoleDto(
                r.CvrNumber,
                r.Company.Name,
                r.Company.Status.ToString(),
                r.Role.ToString(),
                r.StartDate,
                r.EndDate)).ToList());

        await _cache.SetAsync(cacheKey, dto);
        return dto;
    }

    public async Task<List<BankruptcyDto>> GetBankruptciesAsync(string personId, CancellationToken ct = default)
    {
        var cacheKey = CacheKeys.PersonBankruptcies(personId);
        var cached = await _cache.GetAsync<List<BankruptcyDto>>(cacheKey);
        if (cached != null) return cached;

        var roles = await _db.PersonCompanyRoles
            .Include(r => r.Company)
            .Where(r => r.PersonCvrId == personId && r.Company.Status == CompanyStatus.Bankrupt)
            .ToListAsync(ct);

        if (roles.Count == 0)
        {
            await _enrichment.EnrichPersonAsync(personId, ct);
            roles = await _db.PersonCompanyRoles
                .Include(r => r.Company)
                .Where(r => r.PersonCvrId == personId && r.Company.Status == CompanyStatus.Bankrupt)
                .ToListAsync(ct);
        }

        var dtos = roles
            .Select(r => r.Company)
            .DistinctBy(c => c.CvrNumber)
            .Select(c => new BankruptcyDto(c.CvrNumber, c.Name, c.BankruptcyDate))
            .OrderByDescending(d => d.BankruptcyDate)
            .ToList();

        await _cache.SetAsync(cacheKey, dtos);
        return dtos;
    }
}
