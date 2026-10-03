namespace RepositorioRemoto.Sync;

/// <summary>
/// Contrato de la sincronización entre la API remota y el almacenamiento local.
/// Se separa del BackgroundService para poder testear la lógica con mocks (buenas prácticas 22.9).
/// </summary>
public interface IUserSyncService
{
    /// <summary>
    /// Vacía la caché y la base de datos local y las recarga con lo que haya en la API.
    /// </summary>
    /// <returns>true si la sincronización se completó; false si falló la API.</returns>
    Task<bool> SyncAsync();
}