namespace KonkursCheck.Application.DTOs;

public record PersonSearchResultDto(
    string PersonCvrId,
    string FullName,
    int TotalBankruptcies);

public record CompanySearchResultDto(
    string CvrNumber,
    string Name,
    string Status);

public record SearchResponseDto(
    List<PersonSearchResultDto> Persons,
    List<CompanySearchResultDto> Companies);
