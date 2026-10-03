namespace RepositorioRemoto.Dto;

/// <summary>
/// DTO de empresa.
/// </summary>
/// <param name="Name">Nombre de la empresa.</param>
/// <param name="CatchPhrase">Eslogan.</param>
/// <param name="Bs">Lema de negocio.</param>
public record CompanyDto(
    string Name,
    string CatchPhrase,
    string Bs
);