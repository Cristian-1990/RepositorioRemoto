using RepositorioRemoto.Dto;
using RepositorioRemoto.Models;

namespace RepositorioRemoto.Mappers;

/// <summary>
/// Métodos de extensión para convertir entre los DTOs y el modelo de dominio.
/// Centraliza la conversión entre capas.
/// </summary>
public static class UserMapper
{
    /// <summary>Convierte el usuario recibido de la API en el modelo de dominio.</summary>
    public static User ToUser(this JsonPlaceholderUserDto dto) => new(
        dto.Id,
        dto.Name,
        dto.Username,
        dto.Email,
        dto.Address.ToModel(),
        dto.Phone,
        dto.Website,
        dto.Company.ToModel());

    /// <summary>Convierte una petición de creación en User. El id lo asigna la API.</summary>
    public static User ToUser(this CreateUserRequest request, int id = 0) => new(
        id,
        request.Name,
        request.Username,
        request.Email,
        request.Address.ToModel(),
        request.Phone,
        request.Website,
        request.Company.ToModel());

    /// <summary>Convierte una petición de actualización en User.</summary>
    public static User ToUser(this UpdateUserRequest request) => new(
        request.Id,
        request.Name,
        request.Username,
        request.Email,
        request.Address.ToModel(),
        request.Phone,
        request.Website,
        request.Company.ToModel());

    /// <summary>Convierte un User en el DTO de respuesta.</summary>
    public static UserResponseDto ToResponse(this User user) => new(
        user.Id,
        user.Name,
        user.Username,
        user.Email,
        user.Address.ToDto(),
        user.Phone,
        user.Website,
        user.Company.ToDto());

    /// <summary>Convierte un User en la petición de creación que se envía a la API.</summary>
    public static CreateUserRequest ToCreateRequest(this User user) => new(
        user.Name,
        user.Username,
        user.Email,
        user.Address.ToDto(),
        user.Phone,
        user.Website,
        user.Company.ToDto());

    /// <summary>Convierte un User en la petición de actualización que se envía a la API.</summary>
    public static UpdateUserRequest ToUpdateRequest(this User user) => new(
        user.Id,
        user.Name,
        user.Username,
        user.Email,
        user.Address.ToDto(),
        user.Phone,
        user.Website,
        user.Company.ToDto());
    
    public static JsonPlaceholderUserDto ToDto(
        this CreateUserRequest request,
        int id = 0
    ) => new(
        id,
        request.Name,
        request.Username,
        request.Email,
        request.Address,
        request.Phone,
        request.Website,
        request.Company
    );
    
    public static JsonPlaceholderUserDto ToDto(
        this UpdateUserRequest request
    ) => new(
        request.Id,
        request.Name,
        request.Username,
        request.Email,
        request.Address,
        request.Phone,
        request.Website,
        request.Company
    );

    /// <summary>Convierte el DTO de dirección en el modelo.</summary>
    public static Address ToModel(this AddressDto dto) => new(
        dto.Street, dto.Suite, dto.City, dto.Zipcode, dto.Geo.ToModel());

    /// <summary>Convierte el DTO de coordenadas en el modelo.</summary>
    public static Geo ToModel(this GeoDto dto) => new(dto.Lat, dto.Lng);

    /// <summary>Convierte el DTO de empresa en el modelo.</summary>
    public static Company ToModel(this CompanyDto dto) => new(dto.Name, dto.CatchPhrase, dto.Bs);

    /// <summary>Convierte la dirección del modelo en su DTO.</summary>
    public static AddressDto ToDto(this Address address) => new(
        address.Street, address.Suite, address.City, address.Zipcode, address.Geo.ToDto());

    /// <summary>Convierte las coordenadas del modelo en su DTO.</summary>
    public static GeoDto ToDto(this Geo geo) => new(geo.Lat, geo.Lng);

    /// <summary>Convierte la empresa del modelo en su DTO.</summary>
    public static CompanyDto ToDto(this Company company) => new(company.Name, company.CatchPhrase, company.Bs);
}