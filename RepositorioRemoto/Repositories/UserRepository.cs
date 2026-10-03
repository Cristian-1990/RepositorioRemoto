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
    
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        var entities = await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .ToListAsync();

        return entities.ToUser();
    }
    
    public async Task<User?> GetByIdAsync(int id)
    {
        var entity = await _context.Users.FindAsync(id);
        return entity?.ToUser();
    }
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
    
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Users.FindAsync(id);
        if (entity is null) return false;

        _context.Users.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    
    public async Task DeleteAllAsync()
    {
        _context.Users.RemoveRange(_context.Users);
        await _context.SaveChangesAsync();
    }
}