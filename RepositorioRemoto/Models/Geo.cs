namespace RepositorioRemoto.Models;

/// <summary>
/// Coordenadas de la vivienda del usuario
/// </summary>
/// <param name="Lat">Latitud</param>
/// <param name="Lng">Longitud</param>
public record Geo(
    string Lat,
    string Lng
); 
