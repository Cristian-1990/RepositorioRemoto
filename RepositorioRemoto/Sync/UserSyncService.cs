using Microsoft.Extensions.Logging;
using RepositorioRemoto.Cache;
using RepositorioRemoto.Repositories;

namespace RepositorioRemoto.Sync;

/// <summary>
/// Sincroniza el almacenamiento local con la API remota.
/// Sigue el orden del enunciado: vaciar caché, vaciar BD, pedir todos y guardarlos.
/// </summary>
public class UserSyncService(
    IUserRemoteRepository remoteRepository,
    IUserRepository localRepository,
    ICacheService cache,
    ILogger<UserSyncService> logger) : IUserSyncService
{
    /// <inheritdoc />
    public async Task<bool> SyncAsync()
    {
        logger.LogInformation("Iniciando sincronización con la API remota");

        await cache.ClearAsync();
        await localRepository.DeleteAllAsync();

        var resultado = await remoteRepository.GetAllAsync();

        if (resultado.IsFailure)
        {
            logger.LogWarning("Sincronización fallida: {Error}", resultado.Error.Message);
            return false;
        }

        await localRepository.CreateAllAsync(resultado.Value);

        logger.LogInformation("Sincronización completada: {Count} usuarios", resultado.Value.Count);
        return true;
    }
}