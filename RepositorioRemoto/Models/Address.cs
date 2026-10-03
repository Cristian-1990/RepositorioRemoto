namespace RepositorioRemoto.Models;
/// <summary>
/// Direccion de le usuario
/// </summary>
/// <param name="Street">calle</param>
/// <param name="Suite">psio</param>
/// <param name="City">Ciudad</param>
/// <param name="Zipcode">Codigo postal</param>
/// <param name="Geo">Cordenadas de direccion</param>
public record Address(
    string Street,
    string Suite,
    string City,
    string Zipcode,
    Geo Geo
    );