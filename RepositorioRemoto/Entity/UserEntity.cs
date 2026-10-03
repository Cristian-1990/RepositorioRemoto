namespace RepositorioRemoto.Entity;

/// <summary>
/// Entidad de persistencia para usuarios.
/// La dirección y la empresa se guardan desmontadas en columnas de la misma tabla.
/// La configuración (tabla, columnas y longitudes) está en AppDbContext con Fluent API.
/// </summary>
public class UserEntity
{
    /// <summary>Identificador del usuario. Lo asigna la API remota, no la base de datos.</summary>
    public int Id { get; set; }

    /// <summary>Nombre completo.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Nombre de usuario.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Correo electrónico.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Calle de la dirección.</summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>Piso o apartamento.</summary>
    public string Suite { get; set; } = string.Empty;

    /// <summary>Ciudad.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Código postal.</summary>
    public string Zipcode { get; set; } = string.Empty;

    /// <summary>Latitud de la dirección.</summary>
    public string Lat { get; set; } = string.Empty;

    /// <summary>Longitud de la dirección.</summary>
    public string Lng { get; set; } = string.Empty;

    /// <summary>Teléfono.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Página web.</summary>
    public string Website { get; set; } = string.Empty;

    /// <summary>Nombre de la empresa.</summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>Eslogan de la empresa.</summary>
    public string CompanyCatchPhrase { get; set; } = string.Empty;

    /// <summary>Lema de negocio de la empresa.</summary>
    public string CompanyBs { get; set; } = string.Empty;
}