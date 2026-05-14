namespace KonkursCheck.Application.DTOs;

public record RoleDto(
    string CvrNumber,
    string CompanyName,
    string CompanyStatus,
    string Role,
    DateOnly? StartDate,
    DateOnly? EndDate);

public record PersonProfileDto(
    string PersonCvrId,
    string FullName,
    int TotalBankruptcies,
    List<RoleDto> Roles);

public record BankruptcyDto(
    string CvrNumber,
    string CompanyName,
    DateOnly? BankruptcyDate);
