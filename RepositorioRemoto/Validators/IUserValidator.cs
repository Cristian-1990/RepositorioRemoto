using CSharpFunctionalExtensions;
using RepositorioRemoto.Dto;
using RepositorioRemoto.Errors;

namespace RepositorioRemoto.Validators;

/// <summary>
/// Interfaz del  validador de peticiones de usuario.
/// Aunque el repo aconseja no crear interfaces por crear, la creamos para poder testear despues
/// </summary>
public interface IUserValidator
{
    /// <summary>
    /// Comprueba que los datos de creación son correctos.
    /// </summary>
    /// <param name="request">Datos recibidos para crear el usuario.</param>
    /// <returns>petición válida o error de dominio</returns>
    Result<CreateUserRequest, DomainError> ValidateCreate(CreateUserRequest request);

    /// <summary>
    /// Comprueba que los datos de actualización son correctos.
    /// </summary>
    /// <param name="request">Datos para actualizar el usuario.</param>
    /// <returns>Peticion o un error de dominio.</returns>
    Result<UpdateUserRequest, DomainError> ValidateUpdate(UpdateUserRequest request);
}