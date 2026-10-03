namespace RepositorioRemoto.Config;

/// <summary>
/// Configuración de la API REST remota (JSONPlaceholder).
/// </summary>
public class ApiConfig
{
    /// <summary>Nombre de la sección en appsettings.json.</summary>
    public const string SectionName = "ApiSettings";

    /// <summary>URL de la API REST.</summary>
    public string BaseUrl { get; set; } = "https://jsonplaceholder.typicode.com";
}