using CSharpFunctionalExtensions;
using RepositorioRemoto.Dto;
using RepositorioRemoto.Errors;

namespace RepositorioRemoto.Services;

/// <summary>
/// Contrato del servicio de usuarios: orquesta los tres niveles de almacenamiento
/// (caché, base de datos local y API REST remota) y notifica las escrituras.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Obtiene todos los usuarios de la base de datos local.
    /// Si está vacía, los trae de la API remota y los guarda antes de devolverlos.
    /// </summary>
    Task<Result<List<UserResponseDto>, DomainError>> GetAllAsync();

    /// <summary>
    /// Obtiene un usuario buscando en caché, luego en la base de datos local
    /// y por último en la API remota. NotFound si no está en ninguno.
    /// </summary>
    /// <param name="id">Identificador del usuario.</param>
    Task<Result<UserResponseDto, DomainError>> GetByIdAsync(int id);

    /// <summary>
    /// Crea un usuario: lo valida, lo envía a la API remota y guarda
    /// el resultado en la base de datos local y en la caché.
    /// </summary>
    /// <param name="request">Datos del usuario a crear.</param>
    Task<Result<UserResponseDto, DomainError>> CreateAsync(CreateUserRequest request);

    /// <summary>
    /// Actualiza un usuario: lo valida, lo envía a la API remota y refresca
    /// la base de datos local y la caché.
    /// </summary>
    /// <param name="id">Identificador del usuario a actualizar.</param>
    /// <param name="request">Datos nuevos.</param>
    Task<Result<UserResponseDto, DomainError>> UpdateAsync(int id, UpdateUserRequest request);

    /// <summary>
    /// Elimina un usuario de la API remota, de la base de datos local y de la caché.
    /// </summary>
    /// <param name="id">Identificador del usuario a eliminar.</param>
    Task<Result<bool, DomainError>> DeleteAsync(int id);

    /// <summary>
    /// Exporta todos los usuarios a un fichero JSON.
    /// </summary>
    /// <returns>La ruta del fichero generado.</returns>
    Task<Result<string, DomainError>> ExportToJsonAsync();
}
