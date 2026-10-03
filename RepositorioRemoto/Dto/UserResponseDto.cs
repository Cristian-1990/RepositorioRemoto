namespace RepositorioRemoto.Dto;

/// <summary>
/// DTO con los datos de un usuario que el servicio devuelve hacia fuera.
/// </summary>
/// <param name="Id">Identificador.</param>
/// <param name="Name">Nombre completo.</param>
/// <param name="Username">Nombre de usuario.</param>
/// <param name="Email">Correo electrónico.</param>
/// <param name="Address">Dirección.</param>
/// <param name="Phone">Teléfono.</param>
/// <param name="Website">Página web.</param>
/// <param name="Company">Empresa.</param>
public record UserResponseDto(
    int Id,
    string Name,
    string Username,
    string Email,
    AddressDto Address,
    string Phone,
    string Website,
    CompanyDto Company
);