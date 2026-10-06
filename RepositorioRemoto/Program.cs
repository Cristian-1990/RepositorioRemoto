using Microsoft.EntityFrameworkCore;
using Serilog.Events;
using RepositorioRemoto.Dto;
using RepositorioRemoto.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepositorioRemoto.Entity;
using RepositorioRemoto.Infrastructure;
using RepositorioRemoto.Notifications;
using RepositorioRemoto.Sync;
using Serilog;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== Practica 6: Repositorio Remoto ===\n");

// ============================================================
// 1. Perfil de ejecucion (Development o Production)
// ============================================================
var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
Console.WriteLine($"Perfil activo: {environment}\n");

// ============================================================
// 2. Configuracion desde los appsettings
// ============================================================
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// ============================================================
// 3. Logging con Serilog (consola + fichero con rotacion diaria)
// ============================================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/repositorio-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
// ============================================================
// 4. Inyeccion de dependencias
// ============================================================
var provider = DependenciesProvider.BuildServiceProvider(configuration);

// ============================================================
// 5. Suscripcion a las notificaciones
//    IMPORTANTE: antes de cualquier operacion. El observable es
//    CALIENTE: lo que pase antes de suscribirse se pierde.
// ============================================================
var notifications = provider.GetRequiredService<INotificationService>();
using var suscripcion = notifications.Notifications.Subscribe(n =>
    Console.WriteLine($"  [NOTIFICACION {n.Timestamp:HH:mm:ss}] {n.Type}: {n.Message}"));

// ============================================================
// 6. Arranque: crear la BD, vaciarla y cargarla desde la API
// ============================================================
Console.WriteLine("--- Arranque: cargando datos desde la API remota ---");
using (var scope = provider.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.EnsureCreatedAsync();

    var sync = scope.ServiceProvider.GetRequiredService<IUserSyncService>();
    var cargado = await sync.SyncAsync();
    Console.WriteLine(cargado
        ? "  Carga inicial completada"
        : "  La carga inicial ha fallado, se continua con la BD vacia");
}

// ============================================================
// 7. Arrancar la tarea de sincronizacion periodica
// ============================================================
var backgroundService = provider.GetRequiredService<UserSyncBackgroundService>();
using var cts = new CancellationTokenSource();
await backgroundService.StartAsync(cts.Token);
Console.WriteLine("  Sincronizacion periodica arrancada\n");

// ============================================================
// 8. Demostracion de las operaciones
// ============================================================
using (var scope = provider.CreateScope())
{
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

    Console.WriteLine("--- GET: todos los usuarios ---");
    var todos = await userService.GetAllAsync();
    if (todos.IsSuccess)
    {
        Console.WriteLine($"  Total: {todos.Value.Count} usuarios");
        foreach (var u in todos.Value.Take(3))
        {
            Console.WriteLine($"  [{u.Id}] {u.Name} ({u.Username}) - {u.Email}");
        }

        Console.WriteLine("  ...\n");
    }
    else
    {
        Console.WriteLine($"  ERROR: {todos.Error.Message}\n");
    }

    Console.WriteLine("--- GET por id: primera vez (busca en BD local) ---");
    var primera = await userService.GetByIdAsync(1);
    Console.WriteLine(primera.IsSuccess
        ? $"  {primera.Value.Name} - {primera.Value.Email}"
        : $"  ERROR: {primera.Error.Message}");

    Console.WriteLine("--- GET por id: segunda vez (deberia salir de la cache) ---");
    var segunda = await userService.GetByIdAsync(1);
    Console.WriteLine(segunda.IsSuccess
        ? $"  {segunda.Value.Name} - {segunda.Value.Email}\n"
        : $"  ERROR: {segunda.Error.Message}\n");

    Console.WriteLine("--- POST: crear usuario ---");
    var nuevo = new CreateUserRequest(
        "Cristian Alvarez", "crisalvfer", "cristian@gmail.com",
        new AddressDto("Calle Mayor", "2B", "Leganes", "28911",
            new GeoDto("40.3272", "-3.7635")),
        "600123456", "cafeterianero.es",
        new CompanyDto("Cafeteria Nero", "El mejor cafe de Leganes", "coffee-shop"));

    var creado = await userService.CreateAsync(nuevo);
    Console.WriteLine(creado.IsSuccess
        ? $"  Creado con id {creado.Value.Id}\n"
        : $"  ERROR: {creado.Error.Message}\n");

    Console.WriteLine("--- PUT: actualizar el usuario 1 ---");
    var cambios = new UpdateUserRequest(
        1, "Cristian Actualizado", "crisalvfer", "cristian@gmail.com",
        new AddressDto("Calle Mayor", "2B", "Leganes", "28911",
            new GeoDto("40.3272", "-3.7635")),
        "600123456", "cafeterianero.es",
        new CompanyDto("Cafeteria Nero", "El mejor cafe de Leganes", "coffee-shop"));

    var actualizado = await userService.UpdateAsync(1, cambios);
    Console.WriteLine(actualizado.IsSuccess
        ? $"  Actualizado: {actualizado.Value.Name}\n"
        : $"  ERROR: {actualizado.Error.Message}\n");

    Console.WriteLine("--- DELETE: eliminar el usuario 2 ---");
    var borrado = await userService.DeleteAsync(2);
    Console.WriteLine(borrado.IsSuccess
        ? "  Eliminado\n"
        : $"  ERROR: {borrado.Error.Message}\n");

    Console.WriteLine("--- EXPORT: exportar todos a JSON ---");
    var exportado = await userService.ExportToJsonAsync();
    Console.WriteLine(exportado.IsSuccess
        ? $"  Fichero generado en: {exportado.Value}\n"
        : $"  ERROR: {exportado.Error.Message}\n");
}

// ============================================================
// 9. Cierre ordenado
// ============================================================
Console.WriteLine("Pulsa ENTER para detener la aplicacion...");
Console.ReadLine();

await backgroundService.StopAsync(cts.Token);
Console.WriteLine("Aplicacion detenida.");
if (provider is IAsyncDisposable disposable)
{
    await disposable.DisposeAsync();
}
await Log.CloseAndFlushAsync();