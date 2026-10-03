namespace RepositorioRemoto.Config;

/// <summary>
/// Configuración de la exportación de usuarios a JSON.
/// </summary>
public class ExportConfig
{
    /// <summary>Seccion del appsettings.json.</summary>
    public const string SectionName = "ExportSettings";

    /// <summary>Carpeta donde se guarda el fichero exportado.</summary>
    public string OutputDirectory { get; set; } = "output";

    /// <summary>Nombre del fichero exportado.</summary>
    public string FileName { get; set; } = "usuarios.json";
}