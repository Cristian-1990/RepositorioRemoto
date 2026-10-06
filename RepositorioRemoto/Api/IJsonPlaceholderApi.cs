using Refit;
using RepositorioRemoto.Dto;

namespace RepositorioRemoto.Api;

public interface IJsonPlaceholderApi
{
    [Get("/users")]
    Task<List<JsonPlaceholderUserDto>> GetAllAsync();

    [Get("/users/{id}")]
    Task<JsonPlaceholderUserDto> GetByIdAsync(int id);
    
    [Post("/users")]
    Task <JsonPlaceholderUserDto> CreateAsync([Body] JsonPlaceholderUserDto user);
    
    [Put("/users/{id}")]
    Task <JsonPlaceholderUserDto> UpdateAsync(int id, [Body] JsonPlaceholderUserDto user);
    
    [Delete("/users/{id}")]
    Task DeleteAsync(int id);
}