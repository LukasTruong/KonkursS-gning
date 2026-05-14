using System.Net.Http.Json;
using System.Text.Json;

namespace KonkursCheck.Infrastructure.Cvr;

public class CvrElasticClient : ICvrElasticClient
{
    private const string BankruptStatus = "OPLOEST EFTER KONKURS";
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _http;

    public CvrElasticClient(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<CvrPersonResult>> SearchPersonsAsync(string name, CancellationToken ct = default)
    {
        var query = new Dictionary<string, object>
        {
            ["size"] = 20,
            ["query"] = new Dictionary<string, object>
            {
                ["match"] = new Dictionary<string, object>
                {
                    ["Vrdeltagerperson.navne.navn"] = name
                }
            }
        };

        var response = await PostSearchAsync<CvrPerson>("person/_search", query, ct);
        return response
            .Where(h => h.Data != null)
            .Select(h => new CvrPersonResult(
                h.Data!.EnhedsNummer.ToString(),
                h.Data.Navne?.FirstOrDefault()?.Navn ?? "Ukendt"))
            .ToList();
    }

    public async Task<IReadOnlyList<CvrCompanyResult>> SearchCompaniesAsync(string query, CancellationToken ct = default)
    {
        var esQuery = new Dictionary<string, object>
        {
            ["size"] = 20,
            ["query"] = new Dictionary<string, object>
            {
                ["match"] = new Dictionary<string, object>
                {
                    ["Vrvirksomhed.virksomhedMetadata.nyesteNavn.navn"] = query
                }
            }
        };

        var response = await PostSearchAsync<CvrVirksomhed>("virksomhed/_search", esQuery, ct);
        return MapCompanies(response);
    }

    public async Task<IReadOnlyList<CvrCompanyResult>> GetPersonBankruptciesAsync(string personId, CancellationToken ct = default)
    {
        var query = new Dictionary<string, object>
        {
            ["size"] = 100,
            ["query"] = new Dictionary<string, object>
            {
                ["bool"] = new Dictionary<string, object>
                {
                    ["must"] = new List<object>
                    {
                        new Dictionary<string, object>
                        {
                            ["term"] = new Dictionary<string, object>
                            {
                                ["Vrvirksomhed.livsforloeb.status"] = BankruptStatus
                            }
                        },
                        new Dictionary<string, object>
                        {
                            ["nested"] = new Dictionary<string, object>
                            {
                                ["path"] = "Vrvirksomhed.deltagerRelation",
                                ["query"] = new Dictionary<string, object>
                                {
                                    ["term"] = new Dictionary<string, object>
                                    {
                                        ["Vrvirksomhed.deltagerRelation.deltager.enhedsNummer"] = long.Parse(personId)
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        var response = await PostSearchAsync<CvrVirksomhed>("virksomhed/_search", query, ct);
        return MapCompanies(response);
    }

    public async Task<CvrCompanyResult?> GetCompanyAsync(string cvrNumber, CancellationToken ct = default)
    {
        var query = new Dictionary<string, object>
        {
            ["size"] = 1,
            ["query"] = new Dictionary<string, object>
            {
                ["term"] = new Dictionary<string, object>
                {
                    ["Vrvirksomhed.cvrNummer"] = long.Parse(cvrNumber)
                }
            }
        };

        var response = await PostSearchAsync<CvrVirksomhed>("virksomhed/_search", query, ct);
        return MapCompanies(response).FirstOrDefault();
    }

    public async Task<IReadOnlyList<CvrPersonRoleResult>> GetCompanyPersonsAsync(string cvrNumber, CancellationToken ct = default)
    {
        var query = new Dictionary<string, object>
        {
            ["size"] = 1,
            ["query"] = new Dictionary<string, object>
            {
                ["term"] = new Dictionary<string, object>
                {
                    ["Vrvirksomhed.cvrNummer"] = long.Parse(cvrNumber)
                }
            }
        };

        var response = await PostSearchAsync<CvrVirksomhed>("virksomhed/_search", query, ct);
        var virksomhed = response.FirstOrDefault();
        if (virksomhed?.Data?.DeltagerRelationer == null) return [];

        return virksomhed.Data.DeltagerRelationer
            .Where(d => d.Deltager != null)
            .Select(d =>
            {
                var role = ExtractRole(d.Organisationer);
                var (start, end) = ExtractDates(d.Organisationer);
                return new CvrPersonRoleResult(
                    d.Deltager!.EnhedsNummer.ToString(),
                    "Ukendt",
                    role,
                    start,
                    end);
            })
            .ToList();
    }

    public async Task<IReadOnlyList<CvrCompanyResult>> GetAllBankruptCompaniesAsync(int from, int size, CancellationToken ct = default)
    {
        var query = new Dictionary<string, object>
        {
            ["from"] = from,
            ["size"] = size,
            ["query"] = new Dictionary<string, object>
            {
                ["term"] = new Dictionary<string, object>
                {
                    ["Vrvirksomhed.livsforloeb.status"] = BankruptStatus
                }
            }
        };

        var response = await PostSearchAsync<CvrVirksomhed>("virksomhed/_search", query, ct);
        return MapCompanies(response);
    }

    private async Task<List<T>> PostSearchAsync<T>(string path, object query, CancellationToken ct)
    {
        var resp = await _http.PostAsJsonAsync(path, query, JsonOpts, ct);
        resp.EnsureSuccessStatusCode();

        var result = await resp.Content.ReadFromJsonAsync<EsResponse<T>>(JsonOpts, ct);
        return result?.Hits.Hits.Select(h => h.Source).ToList() ?? [];
    }

    private static List<CvrCompanyResult> MapCompanies(List<CvrVirksomhed> hits)
    {
        var results = new List<CvrCompanyResult>();
        foreach (var h in hits)
        {
            if (h.Data == null) continue;
            var name = h.Data.Metadata?.NyesteNavn?.Navn ?? "Ukendt";
            var status = h.Data.Livsforloeb?.LastOrDefault()?.Status ?? "UNKNOWN";
            var founded = ParseDate(h.Data.Metadata?.StiftelsesDato);
            var bankruptcyDate = h.Data.Livsforloeb?
                .Where(l => l.Status == BankruptStatus)
                .Select(l => ParseDate(l.StartDato))
                .FirstOrDefault(d => d != null);
            results.Add(new CvrCompanyResult(
                h.Data.CvrNummer.ToString(),
                name,
                status,
                founded,
                bankruptcyDate,
                h.Data.Branchekode));
        }
        return results;
    }

    private static string ExtractRole(List<CvrOrganisation>? orgs)
    {
        var type = orgs?
            .SelectMany(o => o.MedlemsData ?? [])
            .SelectMany(m => m.Attributter ?? [])
            .FirstOrDefault(a => a.Type?.Equals("FUNKTION", StringComparison.OrdinalIgnoreCase) == true)?
            .Vaerdier?.FirstOrDefault()?.Vaerdi;
        return type ?? "OTHER";
    }

    private static (DateOnly? start, DateOnly? end) ExtractDates(List<CvrOrganisation>? orgs)
    {
        var gyldighed = orgs?
            .SelectMany(o => o.MedlemsData ?? [])
            .SelectMany(m => m.Gyldighed ?? [])
            .FirstOrDefault();
        return (ParseDate(gyldighed?.GyldigFra), ParseDate(gyldighed?.GyldigTil));
    }

    private static DateOnly? ParseDate(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (DateOnly.TryParse(s, out var d)) return d;
        return null;
    }
}
