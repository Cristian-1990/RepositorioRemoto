using RepositorioRemoto.Models;

namespace RepositorioRemoto.Notifications;

/// <summary>
/// Tipo de evento que se notifica.
/// </summary>
public enum NotificationType
{
    /// <summary>Se ha creado un usuario.</summary>
    Create,

    /// <summary>Se ha actualizado un usuario.</summary>
    Update,

    /// <summary>Se ha eliminado un usuario.</summary>
    Delete
}

/// <summary>
/// Evento que viaja por el flujo de notificaciones.
/// </summary>
/// <param name="Type">Tipo de evento.</param>
/// <param name="Message">Mensaje legible para mostrar.</param>
/// <param name="Timestamp">Momento en que se produjo.</param>
public record Notification(NotificationType Type, string Message, DateTime Timestamp);

/// <summary>
/// Contrato del servicio de notificaciones.
/// Emite un flujo de eventos al que Program.cs se suscribe al arrancar.
/// Va inyectado en el UserService, que lo llama al crear, actualizar y eliminar.
/// </summary>
public interface INotificationService
{
    /// <summary>Flujo de notificaciones al que suscribirse.</summary>
    IObservable<Notification> Notifications { get; }

    /// <summary>Notifica que se ha creado un usuario.</summary>
    /// <param name="user">Usuario creado.</param>
    void NotifyCreated(User user);

    /// <summary>Notifica que se ha actualizado un usuario.</summary>
    /// <param name="user">Usuario actualizado.</param>
    void NotifyUpdated(User user);

    /// <summary>Notifica que se ha eliminado un usuario.</summary>
    /// <param name="id">Identificador del usuario eliminado.</param>
    void NotifyDeleted(int id);
}