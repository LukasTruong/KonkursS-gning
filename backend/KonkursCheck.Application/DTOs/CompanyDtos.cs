namespace KonkursCheck.Application.DTOs;

public record CompanyPersonDto(
    string PersonCvrId,
    string FullName,
    string Role,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsCurrent);

public record CompanyProfileDto(
    string CvrNumber,
    string Name,
    string Status,
    DateOnly? FoundedDate,
    DateOnly? BankruptcyDate,
    string? IndustryCode,
    List<CompanyPersonDto> Persons);
