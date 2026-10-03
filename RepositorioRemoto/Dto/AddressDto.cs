namespace RepositorioRemoto.Dto;

/// <summary>
/// DTO de dirección.
/// </summary>
/// <param name="Street">Calle.</param>
/// <param name="Suite">Piso o apartamento.</param>
/// <param name="City">Ciudad.</param>
/// <param name="Zipcode">Código postal.</param>
/// <param name="Geo">Coordenadas.</param>
public record AddressDto(
    string Street,
    string Suite,
    string City,
    string Zipcode,
    GeoDto Geo
);