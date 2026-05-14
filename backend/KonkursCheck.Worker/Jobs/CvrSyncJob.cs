using KonkursCheck.Application.Services;
using KonkursCheck.Infrastructure.Caching;
using KonkursCheck.Infrastructure.Cvr;
using Microsoft.Extensions.Logging;

namespace KonkursCheck.Worker.Jobs;

public class CvrSyncJob
{
    private const int PageSize = 100;

    private readonly ICvrElasticClient _cvr;
    private readonly CvrIntegrationService _integration;
    private readonly EnrichmentService _enrichment;
    private readonly IRedisCacheService _cache;
    private readonly ILogger<CvrSyncJob> _logger;

    public CvrSyncJob(
        ICvrElasticClient cvr,
        CvrIntegrationService integration,
        EnrichmentService enrichment,
        IRedisCacheService cache,
        ILogger<CvrSyncJob> logger)
    {
        _cvr = cvr;
        _integration = integration;
        _enrichment = enrichment;
        _cache = cache;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("CVR sync startet: {Time}", DateTimeOffset.UtcNow);

        int from = 0;
        int updated = 0;
        int errors = 0;
        var affectedPersonIds = new HashSet<string>();

        while (true)
        {
            var companies = await _cvr.GetAllBankruptCompaniesAsync(from, PageSize, ct);
            if (companies.Count == 0) break;

            foreach (var company in companies)
            {
                try
                {
                    await _integration.UpsertCompanyAsync(company, ct);

                    var persons = await _cvr.GetCompanyPersonsAsync(company.CvrNumber, ct);
                    foreach (var person in persons)
                    {
                        await _integration.UpsertPersonAsync(person.PersonId, person.FullName, ct);
                        await _integration.UpsertRoleAsync(person.PersonId, company.CvrNumber, person.Role, person.StartDate, person.EndDate, ct);
                        affectedPersonIds.Add(person.PersonId);
                    }

                    await _cache.RemoveAsync(CacheKeys.Company(company.CvrNumber));
                    await _cache.RemoveAsync(CacheKeys.CompanyPersons(company.CvrNumber));
                    updated++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fejl ved synkronisering af virksomhed {CvrNumber}", company.CvrNumber);
                    errors++;
                }
            }

            from += PageSize;
            if (companies.Count < PageSize) break;
        }

        foreach (var personId in affectedPersonIds)
        {
            try
            {
                await _enrichment.RecalculateBankruptcySummaryAsync(personId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved genberegning af konkursoversigt for person {PersonId}", personId);
            }
        }

        _logger.LogInformation(
            "CVR sync afsluttet: {Updated} virksomheder opdateret, {Errors} fejl, {Persons} personers oversigt genberegnet",
            updated, errors, affectedPersonIds.Count);
    }
}
