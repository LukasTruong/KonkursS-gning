using KonkursCheck.Application.DTOs;
using KonkursCheck.Infrastructure.Caching;
using KonkursCheck.Infrastructure.Cvr;
using KonkursCheck.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KonkursCheck.Application.Services;

public class CompanyService
{
    private readonly AppDbContext _db;
    private readonly ICvrElasticClient _cvr;
    private readonly IRedisCacheService _cache;
    private readonly CvrIntegrationService _integration;

    public CompanyService(AppDbContext db, ICvrElasticClient cvr, IRedisCacheService cache, CvrIntegrationService integration)
    {
        _db = db;
        _cvr = cvr;
        _cache = cache;
        _integration = integration;
    }

    public async Task<CompanyProfileDto?> GetProfileAsync(string cvrNumber, CancellationToken ct = default)
    {
        var cacheKey = CacheKeys.Company(cvrNumber);
        var cached = await _cache.GetAsync<CompanyProfileDto>(cacheKey);
        if (cached != null) return cached;

        var company = await _db.Companies
            .Include(c => c.PersonRoles).ThenInclude(r => r.Person)
            .FirstOrDefaultAsync(c => c.CvrNumber == cvrNumber, ct);

        if (company == null)
        {
            var cvrResult = await _cvr.GetCompanyAsync(cvrNumber, ct);
            if (cvrResult != null)
            {
                await _integration.UpsertCompanyAsync(cvrResult, ct);
                company = await _db.Companies
                    .Include(c => c.PersonRoles).ThenInclude(r => r.Person)
                    .FirstOrDefaultAsync(c => c.CvrNumber == cvrNumber, ct);
            }
        }

        if (company == null) return null;

        var dto = new CompanyProfileDto(
            company.CvrNumber,
            company.Name,
            company.Status.ToString(),
            company.FoundedDate,
            company.BankruptcyDate,
            company.IndustryCode,
            company.PersonRoles.Select(r => new CompanyPersonDto(
                r.PersonCvrId,
                r.Person.FullName,
                r.Role.ToString(),
                r.StartDate,
                r.EndDate,
                r.EndDate == null)).ToList());

        await _cache.SetAsync(cacheKey, dto);
        return dto;
    }

    public async Task<List<CompanyPersonDto>> GetPersonsAsync(string cvrNumber, CancellationToken ct = default)
    {
        var cacheKey = CacheKeys.CompanyPersons(cvrNumber);
        var cached = await _cache.GetAsync<List<CompanyPersonDto>>(cacheKey);
        if (cached != null) return cached;

        var roles = await _db.PersonCompanyRoles
            .Include(r => r.Person)
            .Where(r => r.CvrNumber == cvrNumber)
            .OrderByDescending(r => r.StartDate)
            .ToListAsync(ct);

        if (roles.Count == 0)
        {
            var cvrPersons = await _cvr.GetCompanyPersonsAsync(cvrNumber, ct);
            foreach (var p in cvrPersons)
            {
                await _integration.UpsertPersonAsync(p.PersonId, p.FullName, ct);
                await _integration.UpsertRoleAsync(p.PersonId, cvrNumber, p.Role, p.StartDate, p.EndDate, ct);
            }
            roles = await _db.PersonCompanyRoles
                .Include(r => r.Person)
                .Where(r => r.CvrNumber == cvrNumber)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync(ct);
        }

        var dtos = roles.Select(r => new CompanyPersonDto(
            r.PersonCvrId,
            r.Person.FullName,
            r.Role.ToString(),
            r.StartDate,
            r.EndDate,
            r.EndDate == null)).ToList();

        await _cache.SetAsync(cacheKey, dtos);
        return dtos;
    }
}
