using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RepositorioRemoto.Config;

namespace RepositorioRemoto.Sync;

/// <summary>
/// Tarea en segundo plano que lanza la sincronización cada X segundos.
/// Solo se ocupa del reloj: la lógica está en UserSyncService.
/// </summary>
public class UserSyncBackgroundService(
    IServiceProvider serviceProvider,
    IOptions<SyncConfig> config,
    ILogger<UserSyncBackgroundService> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(config.Value.IntervalSeconds);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Sincronización automática iniciada (cada {Interval})", _interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, stoppingToken);

                using var scope = serviceProvider.CreateScope();
                var syncService = scope.ServiceProvider.GetRequiredService<IUserSyncService>();
                await syncService.SyncAsync();
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Sincronización cancelada al parar la aplicación");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inesperado en la sincronización");
            }
        }

        logger.LogInformation("Sincronización automática detenida");
    }
}