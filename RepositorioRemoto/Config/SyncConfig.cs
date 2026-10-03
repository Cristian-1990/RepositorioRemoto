namespace RepositorioRemoto.Config;

/// <summary>
/// Configuración de la sincronización periódica con la API remota.
/// </summary>
public class SyncConfig
{
    /// <summary>Nombre de la sección en appsettings.json.</summary>
    public const string SectionName = "SyncSettings";

    /// <summary>Segundos entre cada sincronización.</summary>
    public int IntervalSeconds { get; set; } = 60;
}