using System.Text.Json;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RepositorioRemoto.Cache;
using RepositorioRemoto.Config;
using RepositorioRemoto.Dto;
using RepositorioRemoto.Errors;
using RepositorioRemoto.Models;
using RepositorioRemoto.Notification;
using RepositorioRemoto.Repositories;
using RepositorioRemoto.Validators;

namespace RepositorioRemoto.Service;

public class UserService(
    IUserRepository userRepository,
    IUserRemoteRepository remoteRepository,
    ICacheService cacheService,
    IUserValidator userValidator,
    INorificationService notificationService,
    IOptions<ExportConfig> exportOptions,
    ILogger<UserService> logger
    
    ): IUserService

{
    private readonly ExportConfig _exportConfig = exportOptions.Value;
    public async Task<Result<List<User>, DomainError>> GetAllUsers()
    {
        var usuarioLocal = await userRepository.GetAllAsync();
        var listaUsuarios = usuarioLocal.ToList();
        if (usuarioLocal.Any())
        {
            logger.LogInformation("Usuario obtenio desde el repositorio local");
            return Result.Success<List<User>, DomainError>(listaUsuarios);
            
        }
        
        var resultadoRemoto = await remoteRepository.GetAllAsync();

        if (resultadoRemoto.IsFailure)
        {
            return Result.Failure<List<User>, DomainError>(resultadoRemoto.Error);
        }

        var usuariosRemotos = resultadoRemoto.Value;
        await userRepository.CreateAllAsync(usuariosRemotos);
        
        logger.LogInformation("Usuarios Obtenidos del repositorio remoto y guardado en local");
        return Result.Success<List<User>, DomainError>(usuariosRemotos);
    }

    public async Task<Result<User, DomainError>> GetUserById(int id)
    {
        var cacheKey = $"user:{id}";
        var cacheUser = await cacheService.GetAsync<User>(cacheKey);

        if (cacheUser != null)
        {
            logger.LogInformation($"Usuario {id} encontrado en cache");
            return Result.Success<User, DomainError>(cacheUser);
        }
        logger.LogInformation($"Usuario {id} no encontrado en cache");

        var localUser = await userRepository.GetByIdAsync(id);
        if (localUser != null )
        {
            logger.LogInformation("Usuario {id} encontrado en local", id);
            await cacheService.SetAsync(cacheKey, localUser);
            
            return Result.Success<User, DomainError>(localUser);
        }
        
        

        var remoteResult = await remoteRepository.GetByIdAsync(id);
        if(remoteResult.IsFailure)
        {
            return Result.Failure<User,DomainError>(remoteResult.Error);
        }
        var remoteUser = remoteResult.Value;
        
        await userRepository.CreateAsync(remoteUser);
        await cacheService.SetAsync(cacheKey, remoteUser);
        
        logger.LogInformation("Usuario {Id} recuperado del repositorio remoto", id);

        return Result.Success<User, DomainError>(remoteUser);

    }

    public async Task<Result<User, DomainError>> CreateAsync(CreateUserRequest request)
    {
        var validacion = userValidator.ValidateCreate(request);

        if (validacion.IsFailure)
        {
            return Result.Failure<User, DomainError>(validacion.Error);
        }
        
        var remoto = await remoteRepository.CreateAsync(request);

        if (remoto.IsFailure)
        {
            return Result.Failure<User, DomainError>(remoto.Error);
        }

        var usuario = remoto.Value;
        
        var existente = await userRepository.GetByIdAsync(usuario.Id);

        if (existente != null)
        {
            return Result.Failure<User, DomainError>(DomainErrors.AlreadyExists(usuario.Id));
        }
        
        var guardado = await userRepository.CreateAsync(usuario);
        
        await cacheService.SetAsync($"user: {guardado.Id}", guardado);
        notificationService.Notify(new UserNotification(NotificationType.Created,guardado,DateTime.Now));
        logger.LogInformation("Usuario {Id} creado en la cache", guardado.Id);
        
        return Result.Success<User, DomainError>(guardado);
    }

    public async Task<Result<User, DomainError>> UpdateAsync(int id, UpdateUserRequest request)
    {
        var validacion = userValidator.ValidateUpdate(request);
        if (validacion.IsFailure)
        {
            return Result.Failure<User, DomainError>(validacion.Error);
        }
        
        var remoto = await remoteRepository.UpdateAsync(id,request);

        if (remoto.IsFailure)
        {
            return Result.Failure<User, DomainError>(remoto.Error);
        }
        
        var usuarioActualizado = remoto.Value;
        
        var local = await userRepository.UpdateAsync(usuarioActualizado);
        if (local == null)
        {
            return Result.Failure<User, DomainError>(DomainErrors.NotFound(usuarioActualizado.Id));
        }
        
        await cacheService.SetAsync($"user: {local.Id}", local);
        notificationService.Notify(new UserNotification(NotificationType.Update,local,DateTime.Now));
        
        logger.LogInformation("Usuario {Id} actualizado", local.Id);
        
        return Result.Success<User, DomainError>(local);
    }

    public async Task<Result<bool, DomainError>> DeleteAsync(int id)
    {
        var remoto = await remoteRepository.DeleteAsync(id);
        if (remoto.IsFailure)
        {
            return Result.Failure<bool, DomainError>(remoto.Error);
        }
        
        var usuario = await userRepository.GetByIdAsync(id);

        if (usuario == null)
        {
            return Result.Failure<bool, DomainError>(DomainErrors.NotFound(id));
        }
        
        var eliminado = await userRepository.DeleteAsync(id);

        if (!eliminado)
        {
            return Result.Failure<bool, DomainError>(DomainErrors.NotFound(id));
        }
        
        await cacheService.RemoveAsync($"user: {id}");
        notificationService.Notify(new UserNotification(NotificationType.Delete, usuario, DateTime.Now));
        
        logger.LogInformation("Usuario {Id} eliminado", id);
        
        return Result.Success<bool, DomainError>(true);
    }

    public async Task<Result<string, DomainError>> ExportAsync()
    {
        try
        {
            var usuarios = await userRepository.GetAllAsync();
            Directory.CreateDirectory(_exportConfig.OutputDirectory);

            var ruta = Path.Combine(_exportConfig.OutputDirectory, _exportConfig.FileName);
            var json = JsonSerializer.Serialize(usuarios, new JsonSerializerOptions{WriteIndented = true});
            await File.WriteAllTextAsync(ruta, json);
            
            logger.LogInformation("Usuarios exportado A {Ruta}", ruta);
            
            return Result.Success<string, DomainError>(ruta);
        }
        catch (Exception ex)
        {
            return Result.Failure<string, DomainError>(DomainErrors.Storage(ex));
        }
    }
}