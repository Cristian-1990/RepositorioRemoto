using CSharpFunctionalExtensions;
using RepositorioRemoto.Dto;
using RepositorioRemoto.Errors;

namespace RepositorioRemoto.Validators;

/// <summary>
/// Valida los datos de usuario antes de enviarlos a la API remota.
/// Devuelve todos los fallos encontrados.
/// </summary>
public class UserValidator : IUserValidator
{
    /// <summary>
    /// Valida los datos necesarios para crear un usuario.
    /// </summary>
    /// <param name="request">Datos del usuario que se va a crear.</param>
    /// <returns>
    /// El resultado de la validación, con el usuario si es correcto
    /// o un error de dominio si hay fallos.
    /// </returns>
    public Result<CreateUserRequest, DomainError> ValidateCreate(CreateUserRequest request)
    {
        var errores = ComprobarCamposComunes(request.Name, request.Username, request.Email);

        if (errores.Count > 0)
            return Result.Failure<CreateUserRequest, DomainError>(DomainErrors.Validation(errores));

        return Result.Success<CreateUserRequest, DomainError>(request);
    }

    /// <summary>
    /// Valida los datos necesarios para actualizar un usuario.
    /// </summary>
    /// <param name="request">Datos del usuario que se va a actualizar.</param>
    /// <returns>
    /// El resultado de la validación, con el usuario si es correcto
    /// o un error de dominio si hay fallos.
    /// </returns>
    public Result<UpdateUserRequest, DomainError> ValidateUpdate(UpdateUserRequest request)
    {
        var errores = ComprobarCamposComunes(request.Name, request.Username, request.Email);

        if (request.Id <= 0)
            errores.Add("El identificador debe ser mayor que 0");

        if (errores.Count > 0)
            return Result.Failure<UpdateUserRequest, DomainError>(DomainErrors.Validation(errores));

        return Result.Success<UpdateUserRequest, DomainError>(request);
    }

    /// <summary>
    /// Reglas que comparten la creación y la actualización.
    /// </summary>
    /// <param name="name">Nombre completo.</param>
    /// <param name="username">Nombre de usuario.</param>
    /// <param name="email">Correo electrónico.</param>
    /// <returns>Lista con los errores encontrados; vacía si todo es correcto.</returns>
    private static List<string> ComprobarCamposComunes(string name, string username, string email)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
            errores.Add("El nombre es obligatorio");

        if (string.IsNullOrWhiteSpace(username))
            errores.Add("El nombre de usuario es obligatorio");

        if (string.IsNullOrWhiteSpace(email))
            errores.Add("El email es obligatorio");
        else if (!email.Contains('@'))
            errores.Add("El email no tiene un formato válido");

        return errores;
    }
}