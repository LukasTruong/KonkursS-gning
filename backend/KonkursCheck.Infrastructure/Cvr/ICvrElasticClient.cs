namespace KonkursCheck.Infrastructure.Cvr;

public record CvrPersonResult(string PersonId, string FullName);
public record CvrCompanyResult(string CvrNumber, string Name, string Status, DateOnly? FoundedDate, DateOnly? BankruptcyDate, string? IndustryCode);
public record CvrPersonRoleResult(string PersonId, string FullName, string Role, DateOnly? StartDate, DateOnly? EndDate);

public interface ICvrElasticClient
{
    Task<IReadOnlyList<CvrPersonResult>> SearchPersonsAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<CvrCompanyResult>> SearchCompaniesAsync(string query, CancellationToken ct = default);
    Task<IReadOnlyList<CvrCompanyResult>> GetPersonBankruptciesAsync(string personId, CancellationToken ct = default);
    Task<CvrCompanyResult?> GetCompanyAsync(string cvrNumber, CancellationToken ct = default);
    Task<IReadOnlyList<CvrPersonRoleResult>> GetCompanyPersonsAsync(string cvrNumber, CancellationToken ct = default);
    Task<IReadOnlyList<CvrCompanyResult>> GetAllBankruptCompaniesAsync(int from, int size, CancellationToken ct = default);
}
