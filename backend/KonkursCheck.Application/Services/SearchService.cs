using System.Security.Cryptography;
using System.Text;
using KonkursCheck.Application.DTOs;
using KonkursCheck.Domain.Enums;
using KonkursCheck.Infrastructure.Caching;
using KonkursCheck.Infrastructure.Cvr;
using KonkursCheck.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KonkursCheck.Application.Services;

public class SearchService
{
    private readonly AppDbContext _db;
    private readonly ICvrElasticClient _cvr;
    private readonly IRedisCacheService _cache;
    private readonly CvrIntegrationService _integration;

    public SearchService(AppDbContext db, ICvrElasticClient cvr, IRedisCacheService cache, CvrIntegrationService integration)
    {
        _db = db;
        _cvr = cvr;
        _cache = cache;
        _integration = integration;
    }

    public async Task<SearchResponseDto> SearchAsync(string query, string? type, CancellationToken ct = default)
    {
        var cacheKey = CacheKeys.Search(HashQuery(query + type));
        var cached = await _cache.GetAsync<SearchResponseDto>(cacheKey);
        if (cached != null) return cached;

        var persons = new List<PersonSearchResultDto>();
        var companies = new List<CompanySearchResultDto>();

        if (type is null or "person")
        {
            persons = await SearchPersonsAsync(query, ct);
        }
        if (type is null or "company")
        {
            companies = await SearchCompaniesAsync(query, ct);
        }

        var result = new SearchResponseDto(persons, companies);
        await _cache.SetAsync(cacheKey, result);
        return result;
    }

    private async Task<List<PersonSearchResultDto>> SearchPersonsAsync(string query, CancellationToken ct)
    {
        var dbPersons = await _db.Persons
            .Include(p => p.BankruptcySummary)
            .Where(p => EF.Functions.ILike(p.FullName, $"%{query}%"))
            .Take(20)
            .ToListAsync(ct);

        if (dbPersons.Count > 0)
        {
            return dbPersons.Select(p => new PersonSearchResultDto(
                p.PersonCvrId,
                p.FullName,
                p.BankruptcySummary?.TotalBankruptcies ?? 0)).ToList();
        }

        var cvrResults = await _cvr.SearchPersonsAsync(query, ct);
        foreach (var r in cvrResults)
            await _integration.UpsertPersonAsync(r.PersonId, r.FullName, ct);

        return cvrResults.Select(r => new PersonSearchResultDto(r.PersonId, r.FullName, 0)).ToList();
    }

    private async Task<List<CompanySearchResultDto>> SearchCompaniesAsync(string query, CancellationToken ct)
    {
        var dbCompanies = await _db.Companies
            .Where(c => EF.Functions.ILike(c.Name, $"%{query}%"))
            .Take(20)
            .ToListAsync(ct);

        if (dbCompanies.Count > 0)
        {
            return dbCompanies.Select(c => new CompanySearchResultDto(
                c.CvrNumber,
                c.Name,
                c.Status.ToString())).ToList();
        }

        var cvrResults = await _cvr.SearchCompaniesAsync(query, ct);
        foreach (var r in cvrResults)
            await _integration.UpsertCompanyAsync(r, ct);

        return cvrResults.Select(r => new CompanySearchResultDto(
            r.CvrNumber,
            r.Name,
            CvrIntegrationService.MapStatus(r.Status).ToString())).ToList();
    }

    private static string HashQuery(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant()[..16];
    }
}
