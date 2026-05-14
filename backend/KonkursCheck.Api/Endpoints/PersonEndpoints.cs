using KonkursCheck.Application.Services;

namespace KonkursCheck.Api.Endpoints;

public static class PersonEndpoints
{
    public static RouteGroupBuilder MapPersonEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/person/{personCvrId}", async (
            string personCvrId,
            PersonService service,
            CancellationToken ct) =>
        {
            var result = await service.GetProfileAsync(personCvrId, ct);
            return result is null
                ? Results.NotFound(new { error = "Person ikke fundet.", code = 404 })
                : Results.Ok(result);
        });

        group.MapGet("/person/{personCvrId}/bankruptcies", async (
            string personCvrId,
            PersonService service,
            CancellationToken ct) =>
        {
            var result = await service.GetBankruptciesAsync(personCvrId, ct);
            return Results.Ok(result);
        });

        return group;
    }
}
