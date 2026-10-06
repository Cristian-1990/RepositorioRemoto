namespace RepositorioRemoto.Notification;

public interface INorificationService: IDisposable
{
    IObservable<UserNotification> Notifications { get; }
    void Notify(UserNotification notification);
    
}