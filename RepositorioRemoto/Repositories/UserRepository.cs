using Microsoft.EntityFrameworkCore;
using RepositorioRemoto.Entity;
using RepositorioRemoto.Mappers;
using RepositorioRemoto.Models;

namespace RepositorioRemoto.Repositories;

/// <summary>
/// Repositorio local de usuarios con Entity Framework Core.
/// Funciona igual con SQLite (perfil Dev) y con PostgreSQL (perfil Prod):
/// el proveedor se elige al registrar el AppDbContext.
/// </summary>
public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Obtiene todos los usuarios.
    /// </summary>
    /// <returns>Una colección con todos los usuarios.</returns>
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        var entities = await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .ToListAsync();

        return entities.ToUser();
    }

    /// <summary>
    /// Obtiene un usuario por su identificador.
    /// </summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <returns>El usuario encontrado o null si no existe.</returns>
    public async Task<User?> GetByIdAsync(int id)
    {
        var entity = await _context.Users.FindAsync(id);
        return entity?.ToUser();
    }

    /// <summary>
    /// Crea un nuevo usuario.
    /// </summary>
    /// <param name="user">Usuario que se va a crear.</param>
    /// <returns>El usuario creado.</returns>
    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user.ToEntity());
        await _context.SaveChangesAsync();
        return user;
    }

    /// <inheritdoc />
    public async Task CreateAllAsync(IEnumerable<User> users)
    {
        var entities = users.Select(u => u.ToEntity());
        await _context.Users.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Actualiza los datos de un usuario existente.
    /// </summary>
    /// <param name="user">Usuario con los datos actualizados.</param>
    /// <returns>El usuario actualizado o null si no existe.</returns>
    public async Task<User?> UpdateAsync(User user)
    {
        var entity = await _context.Users.FindAsync(user.Id);
        if (entity is null) return null;

        var actualizada = user.ToEntity();

        entity.Name = actualizada.Name;
        entity.Username = actualizada.Username;
        entity.Email = actualizada.Email;
        entity.Street = actualizada.Street;
        entity.Suite = actualizada.Suite;
        entity.City = actualizada.City;
        entity.Zipcode = actualizada.Zipcode;
        entity.Lat = actualizada.Lat;
        entity.Lng = actualizada.Lng;
        entity.Phone = actualizada.Phone;
        entity.Website = actualizada.Website;
        entity.CompanyName = actualizada.CompanyName;
        entity.CompanyCatchPhrase = actualizada.CompanyCatchPhrase;
        entity.CompanyBs = actualizada.CompanyBs;

        await _context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Elimina un usuario por su identificador.
    /// </summary>
    /// <param name="id">Identificador del usuario que se quiere eliminar.</param>
    /// <returns>True si se elimina correctamente; false si el usuario no existe.</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Users.FindAsync(id);
        if (entity is null) return false;

        _context.Users.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Elimina todos los usuarios almacenados.
    /// </summary>
    public async Task DeleteAllAsync()
    {
        _context.Users.RemoveRange(_context.Users);
        await _context.SaveChangesAsync();
    }
}