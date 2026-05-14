using KonkursCheck.Application.Services;

namespace KonkursCheck.Api.Endpoints;

public static class SearchEndpoints
{
    public static RouteGroupBuilder MapSearchEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/search", async (
            string q,
            string? type,
            SearchService service,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.BadRequest(new { error = "Søgeord (q) er påkrævet.", code = 400 });

            var result = await service.SearchAsync(q, type, ct);
            return Results.Ok(result);
        });

        return group;
    }
}
