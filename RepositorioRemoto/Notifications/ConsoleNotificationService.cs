using System.Reactive.Linq;
using System.Reactive.Subjects;
using RepositorioRemoto.Models;

namespace RepositorioRemoto.Notifications;

/// <summary>
/// Servicio de notificaciones con Rx.NET.
/// Observable CALIENTE: quien se suscribe tarde no ve los eventos anteriores.
/// Patrón tomado de la solución 14-ReactividadRxNet (SensorService).
/// </summary>
public class ConsoleNotificationService : INotificationService, IDisposable
{
    private readonly Subject<Notification> _subject = new();

    /// <inheritdoc />
    public IObservable<Notification> Notifications => _subject.AsObservable();

    /// <inheritdoc />
    public void NotifyCreated(User user)
    {
        Emitir(NotificationType.Create, $"Usuario creado: {user.Id} - {user.Name}");
    }

    /// <inheritdoc />
    public void NotifyUpdated(User user)
    {
        Emitir(NotificationType.Update, $"Usuario actualizado: {user.Id} - {user.Name}");
    }

    /// <inheritdoc />
    public void NotifyDeleted(int id)
    {
        Emitir(NotificationType.Delete, $"Usuario eliminado: {id}");
    }

    /// <summary>
    /// Emite la notificación a todos los suscriptores activos en ese momento.
    /// </summary>
    /// <param name="type">Tipo de evento.</param>
    /// <param name="message">Mensaje a mostrar.</param>
    private void Emitir(NotificationType type, string message)
    {
        _subject.OnNext(new Notification(type, message, DateTime.Now));
    }
    
    /// <inheritdoc />
    public void Dispose()
    {
        _subject.OnCompleted();
        _subject.Dispose();
    }
}