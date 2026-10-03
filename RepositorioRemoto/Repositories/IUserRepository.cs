using RepositorioRemoto.Models;

namespace RepositorioRemoto.Repositories;

/// <summary>
/// Contrato del repositorio local de usuarios.
/// Devuelve valores planos: null o false cuando no se encuentra algo.
/// Es el servicio quien los convierte en Result.
/// </summary>
public interface IUserRepository
{
    /// <summary>Obtiene todos los usuarios guardados.</summary>
    Task<IEnumerable<User>> GetAllAsync();

    /// <summary>Obtiene un usuario por su identificador, o null si no existe.</summary>
    Task<User?> GetByIdAsync(int id);

    /// <summary>Guarda un usuario nuevo.</summary>
    Task<User> CreateAsync(User user);

    /// <summary>Guarda varios usuarios de una vez. Lo usa la sincronización.</summary>
    Task CreateAllAsync(IEnumerable<User> users);

    /// <summary>Actualiza un usuario existente, o devuelve null si no existe.</summary>
    Task<User?> UpdateAsync(User user);

    /// <summary>Elimina un usuario. Devuelve false si no existía.</summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>Vacía la tabla. Lo usan el arranque y la sincronización.</summary>
    Task DeleteAllAsync();
}