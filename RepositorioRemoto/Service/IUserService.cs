using CSharpFunctionalExtensions;
using RepositorioRemoto.Dto;
using RepositorioRemoto.Errors;
using RepositorioRemoto.Models;

namespace RepositorioRemoto.Service;

public interface IUserService
{
    Task<Result<List<User>, DomainError>> GetAllUsers();
    
    Task<Result<User, DomainError>> GetUserById(int id);
    
    Task<Result<User, DomainError>> CreateAsync(CreateUserRequest request);
    
    Task<Result<User, DomainError>> UpdateAsync(int id, UpdateUserRequest request);
    
    Task<Result<bool,DomainError>> DeleteAsync(int id);

    Task<Result<string, DomainError>> ExportAsync();
}