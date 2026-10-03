using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RepositorioRemoto.Cache;
using RepositorioRemoto.Config;
using RepositorioRemoto.Dto;
using RepositorioRemoto.Errors;
using RepositorioRemoto.Mappers;
using RepositorioRemoto.Models;
using RepositorioRemoto.Notifications;
using RepositorioRemoto.Repositories;
using RepositorioRemoto.Validators;
using System.Text.Json;

namespace RepositorioRemoto.Services;

/// <summary>
/// Servicio de usuarios. Orquesta los tres niveles de almacenamiento
/// (caché, base de datos local y API REST) y notifica las escrituras.
/// </summary>
public class UserService(
    IUserRepository localRepository,
    IUserRemoteRepository remoteRepository,
    ICacheService cache,
    IUserValidator validator,
    INotificationService notificationService,
    IOptions<ExportConfig> exportOptions,
    ILogger<UserService> logger) : IUserService
{
    private const string CachePrefix = "user:";

    /// <inheritdoc />
    public async Task<Result<List<UserResponseDto>, DomainError>> GetAllAsync()
    {
        var locales = (await localRepository.GetAllAsync()).ToList();
        if (locales.Count > 0)
        {
            return Result.Success<List<UserResponseDto>, DomainError>(
                locales.Select(user => user.ToResponse()).ToList());
        }

        logger.LogInformation("Base de datos local vacia, cargando desde la API remota");

        var remoto = await remoteRepository.GetAllAsync();
        if (remoto.IsFailure)
        {
            return Result.Failure<List<UserResponseDto>, DomainError>(remoto.Error);
        }

        await localRepository.CreateAllAsync(remoto.Value);

        return Result.Success<List<UserResponseDto>, DomainError>(
            remoto.Value.Select(user => user.ToResponse()).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<UserResponseDto, DomainError>> GetByIdAsync(int id)
    {
        var cacheKey = $"{CachePrefix}{id}";

        var cacheado = await cache.GetAsync<User>(cacheKey);
        if (cacheado is not null)
        {
            logger.LogDebug("Cache HIT para el usuario {Id}", id);
            return Result.Success<UserResponseDto, DomainError>(cacheado.ToResponse());
        }

        var local = await localRepository.GetByIdAsync(id);
        if (local is not null)
        {
            await cache.SetAsync(cacheKey, local);
            return Result.Success<UserResponseDto, DomainError>(local.ToResponse());
        }

        var remoto = await remoteRepository.GetByIdAsync(id);
        if (remoto.IsFailure)
        {
            return Result.Failure<UserResponseDto, DomainError>(remoto.Error);
        }

        await localRepository.CreateAsync(remoto.Value);
        await cache.SetAsync(cacheKey, remoto.Value);

        return Result.Success<UserResponseDto, DomainError>(remoto.Value.ToResponse());
    }

    /// <inheritdoc />
    public async Task<Result<UserResponseDto, DomainError>> CreateAsync(CreateUserRequest request)
    {
        var validacion = validator.ValidateCreate(request);
        if (validacion.IsFailure)
        {
            return Result.Failure<UserResponseDto, DomainError>(validacion.Error);
        }

        var remoto = await remoteRepository.CreateAsync(request);
        if (remoto.IsFailure)
        {
            return Result.Failure<UserResponseDto, DomainError>(remoto.Error);
        }

        var creado = remoto.Value;

        var existente = await localRepository.GetByIdAsync(creado.Id);
        if (existente is not null)
        {
            logger.LogWarning("La API ha devuelto el id {Id}, que ya existe en local", creado.Id);
            return Result.Failure<UserResponseDto, DomainError>(DomainErrors.AlreadyExists(creado.Id));
        }

        await localRepository.CreateAsync(creado);
        await cache.SetAsync($"{CachePrefix}{creado.Id}", creado);
        notificationService.NotifyCreated(creado);

        return Result.Success<UserResponseDto, DomainError>(creado.ToResponse());
    }

    /// <inheritdoc />
    public async Task<Result<UserResponseDto, DomainError>> UpdateAsync(int id, UpdateUserRequest request)
    {
        var validacion = validator.ValidateUpdate(request);
        if (validacion.IsFailure)
        {
            return Result.Failure<UserResponseDto, DomainError>(validacion.Error);
        }

        var remoto = await remoteRepository.UpdateAsync(id, request);
        if (remoto.IsFailure)
        {
            return Result.Failure<UserResponseDto, DomainError>(remoto.Error);
        }

        var actualizado = remoto.Value;

        var local = await localRepository.UpdateAsync(actualizado);
        if (local is null)
        {
            logger.LogWarning("El usuario {Id} se actualizo en la API pero no estaba en local", id);
        }

        await cache.SetAsync($"{CachePrefix}{id}", actualizado);
        notificationService.NotifyUpdated(actualizado);

        return Result.Success<UserResponseDto, DomainError>(actualizado.ToResponse());
    }

    /// <inheritdoc />
    public async Task<Result<bool, DomainError>> DeleteAsync(int id)
    {
        var remoto = await remoteRepository.DeleteAsync(id);
        if (remoto.IsFailure)
        {
            return Result.Failure<bool, DomainError>(remoto.Error);
        }

        var borrado = await localRepository.DeleteAsync(id);
        if (!borrado)
        {
            logger.LogWarning("El usuario {Id} se borro en la API pero no estaba en local", id);
        }

        await cache.RemoveAsync($"{CachePrefix}{id}");
        notificationService.NotifyDeleted(id);

        return Result.Success<bool, DomainError>(true);
    }

    /// <inheritdoc />
        public async Task<Result<string, DomainError>> ExportToJsonAsync()
        {
            var usuarios = await GetAllAsync();
            if (usuarios.IsFailure)
            {
                return Result.Failure<string, DomainError>(usuarios.Error);
            }

            var config = exportOptions.Value;

            try
            {
                Directory.CreateDirectory(config.OutputDirectory);
                var ruta = Path.Combine(config.OutputDirectory, config.FileName);

                var opciones = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(usuarios.Value, opciones);
                await File.WriteAllTextAsync(ruta, json);

                logger.LogInformation("Usuarios exportados a {Ruta}", ruta);

                return Result.Success<string, DomainError>(ruta);
            }
            catch (IOException ex)
            {
                return Result.Failure<string, DomainError>(DomainErrors.Storage(ex));
            }
        }
}

