namespace RepositorioRemoto.Errors;

/// <summary>
/// Errores de dominio.
/// Cada tipo de error construye su propio mensaje.
/// </summary>
/// <param name="Message">Mensaje.</param>
public abstract record DomainError(string Message)
{
    /// <summary>cuando no existe ningún usuario con ese identificador.</summary>
    /// <param name="Id">Identificador buscado.</param>
    public sealed record NotFound(int Id)
        : DomainError($"No se ha encontrado ningún usuario con el identificador: {Id}");

    /// <summary>Los datos no cumplen con el validador</summary>
    /// <param name="Errors">Lista de errores detectados.</param>
    public sealed record Validation(IEnumerable<string> Errors)
        : DomainError(string.Join(", ", Errors));

    /// <summary>Ya existe un usuario con ese identificador</summary>
    /// <param name="Id">Identificador</param>
    public sealed record AlreadyExists(int Id)
        : DomainError($"Ya existe un usuario con el identificador: {Id}");

    /// <summary>Fallo al comunicarse con la API REST</summary>
    /// <param name="StatusCode">Código que devuelve la API.</param>
    /// <param name="Detail">Detalle del error.</param>
    public sealed record ApiError(int StatusCode, string Detail)
        : DomainError($"Error en la API remota ({StatusCode}): {Detail}");

    /// <summary>Fallo en el almacenamiento local</summary>
    /// <param name="Exception"></param>
    public sealed record Storage(Exception Exception)
        : DomainError($"Error de almacenamiento: {Exception.Message}");
}

/// <summary>
/// Factoría para crear errores
///  (ver tema 15.4).
/// </summary>
public static class DomainErrors
{
    /// <summary>Usuario no encontrado.</summary>
    public static DomainError NotFound(int id) => new DomainError.NotFound(id);

    /// <summary>validación con la lista de errores.</summary>
    public static DomainError Validation(IEnumerable<string> errors) => new DomainError.Validation(errors);

    /// <summary>usuario ya existente.</summary>
    public static DomainError AlreadyExists(int id) => new DomainError.AlreadyExists(id);

    /// <summary>error de la API</summary>
    public static DomainError ApiError(int statusCode, string detail) => new DomainError.ApiError(statusCode, detail);

    /// <summary>Error de almacenamiento local.</summary>
    public static DomainError Storage(Exception exception) => new DomainError.Storage(exception);
}