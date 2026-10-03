namespace RepositorioRemoto.Dto;

/// <summary>
/// DTO de coordenadas.
/// </summary>
/// <param name="Lat">Latitud.</param>
/// <param name="Lng">Longitud.</param>
public record GeoDto(
    string Lat,
    string Lng
);