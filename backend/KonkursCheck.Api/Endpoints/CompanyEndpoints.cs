using KonkursCheck.Application.Services;

namespace KonkursCheck.Api.Endpoints;

public static class CompanyEndpoints
{
    public static RouteGroupBuilder MapCompanyEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/company/{cvrNumber}", async (
            string cvrNumber,
            CompanyService service,
            CancellationToken ct) =>
        {
            var result = await service.GetProfileAsync(cvrNumber, ct);
            return result is null
                ? Results.NotFound(new { error = "Virksomhed ikke fundet.", code = 404 })
                : Results.Ok(result);
        });

        group.MapGet("/company/{cvrNumber}/persons", async (
            string cvrNumber,
            CompanyService service,
            CancellationToken ct) =>
        {
            var result = await service.GetPersonsAsync(cvrNumber, ct);
            return Results.Ok(result);
        });

        return group;
    }
}
