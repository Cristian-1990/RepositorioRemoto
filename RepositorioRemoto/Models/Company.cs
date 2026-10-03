namespace RepositorioRemoto.Models;
/// <summary>
/// Empresa en la que trabaja cada usuario
/// </summary>
/// <param name="Name">Nombre de la empresa</param>
/// <param name="CatchPhrase">Eslogan</param>
/// <param name="Bs">Lema de la empresa</param>
public record Company(
    string Name,
    string CatchPhrase,
    string Bs
    );