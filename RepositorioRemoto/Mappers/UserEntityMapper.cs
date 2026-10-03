using RepositorioRemoto.Entity;
using RepositorioRemoto.Models;

namespace RepositorioRemoto.Mappers;

/// <summary>
/// Métodos de extensión para convertir entre la entidad de persistencia y el modelo de dominio.
/// </summary>
public static class UserEntityMapper
{
    /// <summary>Convierte una fila de la tabla en el modelo de dominio.</summary>
    public static User ToUser(this UserEntity entity) => new(
        entity.Id,
        entity.Name,
        entity.Username,
        entity.Email,
        new Address(
            entity.Street,
            entity.Suite,
            entity.City,
            entity.Zipcode,
            new Geo(entity.Lat, entity.Lng)),
        entity.Phone,
        entity.Website,
        new Company(
            entity.CompanyName,
            entity.CompanyCatchPhrase,
            entity.CompanyBs));

    /// <summary>Convierte el modelo de dominio en una fila de la tabla.</summary>
    public static UserEntity ToEntity(this User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Username = user.Username,
        Email = user.Email,
        Street = user.Address.Street,
        Suite = user.Address.Suite,
        City = user.Address.City,
        Zipcode = user.Address.Zipcode,
        Lat = user.Address.Geo.Lat,
        Lng = user.Address.Geo.Lng,
        Phone = user.Phone,
        Website = user.Website,
        CompanyName = user.Company.Name,
        CompanyCatchPhrase = user.Company.CatchPhrase,
        CompanyBs = user.Company.Bs
    };

    /// <summary>Convierte una colección de filas en una lista de usuarios.</summary>
    public static List<User> ToUser(this IEnumerable<UserEntity> entities)
        => entities.Select(e => e.ToUser()).ToList();
}