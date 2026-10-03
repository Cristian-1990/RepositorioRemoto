namespace RepositorioRemoto.Cache;

/// <summary>
/// Contrato de la caché. Lo implementan MemoryCacheService (perfil Dev)
/// y RedisCacheService (perfil Prod).
/// </summary>
public interface ICacheService
{
    /// <summary>Obtiene un valor de la caché, o null si no está.</summary>
    /// <param name="key">Clave del valor, por ejemplo "user:1".</param>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>Guarda un valor en la caché.</summary>
    /// <param name="key">Clave del valor.</param>
    /// <param name="value">Valor a guardar.</param>
    /// <param name="expiration">Tiempo que vive en la caché. Si es null, se usa el de la configuración.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    /// <summary>Borra un valor de la caché. Devuelve false si no estaba.</summary>
    /// <param name="key">Clave del valor.</param>
    Task<bool> RemoveAsync(string key);

    /// <summary>Vacía la caché entera. Lo usan el arranque y la sincronización.</summary>
    Task ClearAsync();
}