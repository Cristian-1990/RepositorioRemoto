namespace RepositorioRemoto.Config;

/// <summary>
/// Configuración de la caché.
/// </summary>
public class CacheConfig
{
    /// <summary>Nombre de la sección en appsettings.json.</summary>
    public const string SectionName = "CacheSettings";

    /// <summary>Tipo de caché: "Memory" (perfil Dev) o "Redis" (perfil Prod).</summary>
    public string Provider { get; set; } = "Memory";

    /// <summary>Cadena de conexión a Redis (solo se usa si Provider es "Redis").</summary>
    public string RedisConnectionString { get; set; } = "localhost:6379";

    /// <summary>Segundos que un usuario permanece en la caché.</summary>
    public int ExpirationSeconds { get; set; } = 60;
}