using Refit;
using RepositorioRemoto.Dto;

namespace RepositorioRemoto.Api;

/// <summary>
/// Cliente tipado de la API REST de JSONPlaceholder.
/// Refit genera la implementación en tiempo de ejecución a partir de los atributos
/// [Get], [Post], [Put] y [Delete]: no hay que escribir ni una línea de HttpClient.
/// </summary>
public interface IJsonPlaceholderApi
{
    /// <summary>Obtiene todos los usuarios.</summary>
    [Get("/users")]
    Task<List<JsonPlaceholderUserDto>> GetUsersAsync();

    /// <summary>Obtiene un usuario por su identificador.</summary>
    /// <param name="id">Identificador del usuario.</param>
    [Get("/users/{id}")]
    Task<JsonPlaceholderUserDto?> GetUserByIdAsync(int id);

    /// <summary>Crea un usuario. La API devuelve el usuario con el id que le asigna.</summary>
    /// <param name="request">Datos del usuario a crear.</param>
    [Post("/users")]
    Task<JsonPlaceholderUserDto> CreateUserAsync([Body] CreateUserRequest request);

    /// <summary>Actualiza un usuario existente.</summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <param name="request">Datos nuevos.</param>
    [Put("/users/{id}")]
    Task<JsonPlaceholderUserDto> UpdateUserAsync(int id, [Body] UpdateUserRequest request);

    /// <summary>Elimina un usuario.</summary>
    /// <param name="id">Identificador del usuario.</param>
    [Delete("/users/{id}")]
    Task DeleteUserAsync(int id);
}