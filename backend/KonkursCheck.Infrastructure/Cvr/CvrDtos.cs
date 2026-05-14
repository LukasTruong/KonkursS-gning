using System.Text.Json.Serialization;

namespace KonkursCheck.Infrastructure.Cvr;

// ElasticSearch envelope
public record EsResponse<T>(
    [property: JsonPropertyName("hits")] EsHits<T> Hits);

public record EsHits<T>(
    [property: JsonPropertyName("hits")] List<EsHit<T>> Hits);

public record EsHit<T>(
    [property: JsonPropertyName("_source")] T Source);

// Person index
public record CvrPerson(
    [property: JsonPropertyName("Vrdeltagerperson")] CvrPersonData? Data);

public record CvrPersonData(
    [property: JsonPropertyName("enhedsNummer")] long EnhedsNummer,
    [property: JsonPropertyName("navne")] List<CvrNavn>? Navne);

public record CvrNavn(
    [property: JsonPropertyName("navn")] string? Navn);

// Virksomhed index
public record CvrVirksomhed(
    [property: JsonPropertyName("Vrvirksomhed")] CvrVirksomhedData? Data);

public record CvrVirksomhedData(
    [property: JsonPropertyName("cvrNummer")] long CvrNummer,
    [property: JsonPropertyName("virksomhedMetadata")] CvrVirksomhedMetadata? Metadata,
    [property: JsonPropertyName("livsforloeb")] List<CvrLivsforloeb>? Livsforloeb,
    [property: JsonPropertyName("branchekode")] string? Branchekode,
    [property: JsonPropertyName("deltagerRelation")] List<CvrDeltagerRelation>? DeltagerRelationer);

public record CvrVirksomhedMetadata(
    [property: JsonPropertyName("nyesteNavn")] CvrNavn? NyesteNavn,
    [property: JsonPropertyName("stiftelsesDato")] string? StiftelsesDato);

public record CvrLivsforloeb(
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("startDato")] string? StartDato,
    [property: JsonPropertyName("slutDato")] string? SlutDato);

public record CvrDeltagerRelation(
    [property: JsonPropertyName("deltager")] CvrDeltager? Deltager,
    [property: JsonPropertyName("organisationer")] List<CvrOrganisation>? Organisationer);

public record CvrDeltager(
    [property: JsonPropertyName("enhedsNummer")] long EnhedsNummer);

public record CvrOrganisation(
    [property: JsonPropertyName("medlemsData")] List<CvrMedlemsData>? MedlemsData);

public record CvrMedlemsData(
    [property: JsonPropertyName("attributter")] List<CvrAttribut>? Attributter,
    [property: JsonPropertyName("gyldighed")] List<CvrGyldighed>? Gyldighed);

public record CvrAttribut(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("vaerdier")] List<CvrVaerdi>? Vaerdier);

public record CvrVaerdi(
    [property: JsonPropertyName("vaerdi")] string? Vaerdi);

public record CvrGyldighed(
    [property: JsonPropertyName("gyldigFra")] string? GyldigFra,
    [property: JsonPropertyName("gyldigTil")] string? GyldigTil);
