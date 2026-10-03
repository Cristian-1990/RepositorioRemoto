namespace RepositorioRemoto.Models;

/// <summary>
///  Modelo de dominio que representa un usuario.
/// </summary>
/// <param name="Id">Id </param>
/// <param name="Name">Nombre</param>
/// <param name="Username">Nombre de ususario</param>
/// <param name="Email">Direccion email</param>
/// <param name="Address">Residencia</param>
/// <param name="Phone">Numero de telefono</param>
/// <param name="Website">pagina web</param>
/// <param name="Company">Empresa para la quie trabaja</param>
public record User(
    int Id,
    string Name,
    string Username,
    string Email,
    Address Address,
    string Phone,
    string Website,
    Company Company
);