using System.Net;
using CSharpFunctionalExtensions;
using Refit;
using RepositorioRemoto.Api;
using RepositorioRemoto.Dto;
using RepositorioRemoto.Errors;
using RepositorioRemoto.Mappers;
using RepositorioRemoto.Models;

namespace RepositorioRemoto.Repositories;

public class UsereRemoteRepository(IJsonPlaceholderApi api):IUserRemoteRepository
{
    public async Task<Result<List<User>, DomainError>> GetAllAsync()
    {
        try
        {
            var userDto = await  api.GetAllAsync();
            var usuario = userDto.Select(dto => dto.ToUser()).ToList();

            return Result.Success<List<User>, DomainError>(usuario);
        }
        catch (ApiException ex)
        {
            return Result.Failure<List<User>, DomainError>(DomainErrors.ApiError((int)ex.StatusCode, ex.Message));
        }
        
    }

    public async Task<Result<User, DomainError>> GetByIdAsync(int id)
    {
        try
        {
            var dto = await api.GetByIdAsync(id);
            var usuario = dto.ToUser();

            return Result.Success<User, DomainError>(usuario);
        }
        catch (ApiException ex) when(ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Failure<User, DomainError>(DomainErrors.NotFound(id));
        }
    }

    public async Task<Result<User, DomainError>> CreateAsync(CreateUserRequest request)
    {
        try
        {
            var dto = await api.CreateAsync(request.ToDto());
            var usuario = dto.ToUser();
            return Result.Success<User, DomainError>(usuario);
        }
        catch (ApiException ex)
        {
            return Result.Failure<User, DomainError>(DomainErrors.ApiError((int)ex.StatusCode, ex.Message));
        }
    }

    public async Task<Result<User, DomainError>> UpdateAsync(int id, UpdateUserRequest request)
    {
        try
        {
            var requestDto = request.ToDto();
            var responseDto = await api.UpdateAsync(id, requestDto);

            var usuario = responseDto.ToUser();
            return Result.Success<User, DomainError>(usuario);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Failure<User, DomainError>(DomainErrors.NotFound(id));
        }
        catch (ApiException ex)
        {
            return Result.Failure<User, DomainError>(DomainErrors.ApiError((int)ex.StatusCode, ex.Message));
        }
    }

    public async Task<Result<bool, DomainError>> DeleteAsync(int id)
    {
        try
        {
            await api.DeleteAsync(id);
            return Result.Success<bool, DomainError>(true);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Failure<bool, DomainError>(DomainErrors.NotFound(id));
        }
        catch (ApiException ex)
        {
            return Result.Failure<bool, DomainError>(DomainErrors.ApiError((int)ex.StatusCode, ex.Message));
        }
    }
}