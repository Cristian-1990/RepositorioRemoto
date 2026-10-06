using System.Reactive.Subjects;

namespace RepositorioRemoto.Notification;

public class ConsoleNotificationService:INorificationService
{
    private readonly Subject<UserNotification> _subject = new();
    public IObservable<UserNotification> Notifications => _subject;
    public void Notify(UserNotification notification)
    {
        Console.WriteLine($"{notification.Type} - {notification.User} - {notification.Date}");
        _subject.OnNext(notification);
    }
    
    
    
    public void Dispose()
    {
        _subject.OnCompleted();
        _subject.Dispose();
    }

    
}