using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using RepositorioRemoto.Api;
using RepositorioRemoto.Cache;
using RepositorioRemoto.Config;
using RepositorioRemoto.Entity;
using RepositorioRemoto.Notifications;
using RepositorioRemoto.Repositories;
using RepositorioRemoto.Services;
using RepositorioRemoto.Sync;
using RepositorioRemoto.Validators;
using Serilog;
using StackExchange.Redis;

namespace RepositorioRemoto.Infrastructure;

/// <summary>
/// Centraliza el registro de dependencias de la aplicación.
/// Patrón de la teoría 11.8.1 y de la solución 01-InyeccionDependencias.
/// </summary>
public static class DependenciesProvider
{
    /// <summary>
    /// Construye el contenedor de dependencias a partir de la configuración.
    /// </summary>
    /// <param name="configuration">Configuración cargada de los appsettings.</param>
    public static IServiceProvider BuildServiceProvider(IConfiguration configuration)
    {
        var services = new ServiceCollection();

        var apiConfig = new ApiConfig();
        configuration.GetSection(ApiConfig.SectionName).Bind(apiConfig);

        var cacheConfig = new CacheConfig();
        configuration.GetSection(CacheConfig.SectionName).Bind(cacheConfig);

        var databaseConfig = new DatabaseConfig();
        configuration.GetSection(DatabaseConfig.SectionName).Bind(databaseConfig);

        services.Configure<ApiConfig>(configuration.GetSection(ApiConfig.SectionName));
        services.Configure<CacheConfig>(configuration.GetSection(CacheConfig.SectionName));
        services.Configure<DatabaseConfig>(configuration.GetSection(DatabaseConfig.SectionName));
        services.Configure<SyncConfig>(configuration.GetSection(SyncConfig.SectionName));
        services.Configure<ExportConfig>(configuration.GetSection(ExportConfig.SectionName));

        services.AddLogging(builder => builder.AddSerilog(dispose: true));

        services.AddDbContext<AppDbContext>(options =>
        {
            if (databaseConfig.Provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(databaseConfig.ConnectionString);
            }
            else
            {
                options.UseSqlite(databaseConfig.ConnectionString);
            }
        });

        services.AddHttpClient("jsonplaceholder", client =>
            {
                client.BaseAddress = new Uri(apiConfig.BaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddRefitClient<IJsonPlaceholderApi>();

        services.AddMemoryCache();

        if (cacheConfig.Provider.Equals("Redis", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(cacheConfig.RedisConnectionString));
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }

        services.AddSingleton<IUserValidator, UserValidator>();
        services.AddSingleton<INotificationService, ConsoleNotificationService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRemoteRepository, UserRemoteRepository>();
        services.AddScoped<IUserSyncService, UserSyncService>();
        services.AddScoped<IUserService, UserService>();

        services.AddSingleton<UserSyncBackgroundService>();

        return services.BuildServiceProvider();
    }
}