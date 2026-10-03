namespace RepositorioRemoto.Dto;

/// <summary>
/// DTO con los datos para crear un usuario.
/// El Id no se envía: lo asigna la API remota.
/// </summary>
/// <param name="Name">Nombre completo.</param>
/// <param name="Username">Nombre de usuario.</param>
/// <param name="Email">Correo electrónico.</param>
/// <param name="Address">Dirección.</param>
/// <param name="Phone">Teléfono.</param>
/// <param name="Website">Página web.</param>
/// <param name="Company">Empresa.</param>
public record CreateUserRequest(
    string Name,
    string Username,
    string Email,
    AddressDto Address,
    string Phone,
    string Website,
    CompanyDto Company
);