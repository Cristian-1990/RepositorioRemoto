using CSharpFunctionalExtensions;
using RepositorioRemoto.Dto;
using RepositorioRemoto.Errors;
using RepositorioRemoto.Models;

namespace RepositorioRemoto.Repositories;

/// <summary>
/// Contrato del repositorio remoto: habla con la API REST de JSONPlaceholder.
/// Envuelve al cliente Refit y traduce los fallos HTTP en DomainError.ApiError,
/// para que el resto de la aplicación no trabaje con excepciones.
/// </summary>
public interface IUserRemoteRepository
{
    /// <summary>Obtiene todos los usuarios de la API.</summary>
    Task<Result<List<User>, DomainError>> GetAllAsync();

    /// <summary>Obtiene un usuario por su identificador. NotFound si la API no lo tiene.</summary>
    /// <param name="id">Identificador del usuario.</param>
    Task<Result<User, DomainError>> GetByIdAsync(int id);

    /// <summary>Crea un usuario en la API. Devuelve el usuario con el id que asigna la API.</summary>
    /// <param name="request">Datos del usuario a crear.</param>
    Task<Result<User, DomainError>> CreateAsync(CreateUserRequest request);

    /// <summary>Actualiza un usuario en la API.</summary>
    /// <param name="id">Identificador del usuario a actualizar.</param>
    /// <param name="request">Datos nuevos.</param>
    Task<Result<User, DomainError>> UpdateAsync(int id, UpdateUserRequest request);

    /// <summary>Elimina un usuario de la API.</summary>
    /// <param name="id">Identificador del usuario a eliminar.</param>
    Task<Result<bool, DomainError>> DeleteAsync(int id);
}