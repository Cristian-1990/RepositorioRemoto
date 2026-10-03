using System.Net;
using CSharpFunctionalExtensions;
using Refit;
using RepositorioRemoto.Api;
using RepositorioRemoto.Dto;
using RepositorioRemoto.Errors;
using RepositorioRemoto.Mappers;
using RepositorioRemoto.Models;

namespace RepositorioRemoto.Repositories;

/// <summary>
/// Repositorio remoto sobre el cliente Refit de JSONPlaceholder.
/// Traduce los fallos HTTP en DomainError para que el resto de la aplicación
/// no trabaje con excepciones (patrón de la solución 07-Refit, tema 15).
/// </summary>
public class UserRemoteRepository(IJsonPlaceholderApi api) : IUserRemoteRepository
{
    /// <inheritdoc />
    public async Task<Result<List<User>, DomainError>> GetAllAsync()
    {
        try
        {
            var dtos = await api.GetUsersAsync();
            var users = dtos.Select(dto => dto.ToUser()).ToList();
            return Result.Success<List<User>, DomainError>(users);
        }
        catch (ApiException ex)
        {
            return Result.Failure<List<User>, DomainError>(
                DomainErrors.ApiError((int)ex.StatusCode, ex.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<User, DomainError>> GetByIdAsync(int id)
    {
        try
        {
            var dto = await api.GetUserByIdAsync(id);
            return dto is not null
                ? Result.Success<User, DomainError>(dto.ToUser())
                : Result.Failure<User, DomainError>(DomainErrors.NotFound(id));
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Failure<User, DomainError>(DomainErrors.NotFound(id));
        }
        catch (ApiException ex)
        {
            return Result.Failure<User, DomainError>(
                DomainErrors.ApiError((int)ex.StatusCode, ex.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<User, DomainError>> CreateAsync(CreateUserRequest request)
    {
        try
        {
            var creado = await api.CreateUserAsync(request);
            return Result.Success<User, DomainError>(creado.ToUser());
        }
        catch (ApiException ex)
        {
            return Result.Failure<User, DomainError>(
                DomainErrors.ApiError((int)ex.StatusCode, ex.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<User, DomainError>> UpdateAsync(int id, UpdateUserRequest request)
    {
        try
        {
            var actualizado = await api.UpdateUserAsync(id, request);
            return Result.Success<User, DomainError>(actualizado.ToUser());
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Failure<User, DomainError>(DomainErrors.NotFound(id));
        }
        catch (ApiException ex)
        {
            return Result.Failure<User, DomainError>(
                DomainErrors.ApiError((int)ex.StatusCode, ex.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool, DomainError>> DeleteAsync(int id)
    {
        try
        {
            await api.DeleteUserAsync(id);
            return Result.Success<bool, DomainError>(true);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Failure<bool, DomainError>(DomainErrors.NotFound(id));
        }
        catch (ApiException ex)
        {
            return Result.Failure<bool, DomainError>(
                DomainErrors.ApiError((int)ex.StatusCode, ex.Message));
        }
    }
}