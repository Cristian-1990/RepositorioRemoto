using FluentAssertions;
using RepositorioRemoto.Models;
using RepositorioRemoto.Notification;

namespace RepositorioRemoto.Tests.Notification;

[TestFixture]
[TestOf(typeof(ConsoleNotificationService))]
public class ConsoleNotificationServiceTest
{

    private ConsoleNotificationService _service = null!;


    [SetUp]
    public void Setup()
    {
        _service = new ConsoleNotificationService();
    }

    [TearDown]
    public void TearDown()
    {
        _service.Dispose();
    }
    
    [Test]
    public void Notify_EnviarNotificacion()
    {
        UserNotification? recibida = null;
        var usuario = CrearUsuario(1);
        _service.Notifications.Subscribe(notification => { recibida = notification; });
        var notification = new UserNotification(NotificationType.Created, usuario, DateTime.Now);
        
        _service.Notify(notification);
        recibida.Should().NotBeNull();
        recibida!.Type.Should().Be(NotificationType.Created);
        recibida.User.Should().BeEquivalentTo(usuario);
    }

    [Test]
    public void Notify_NotificationUpdate()
    {
        UserNotification? recibida = null;
        var usuario = CrearUsuario(1);
        
        _service.Notifications.Subscribe(notification => { recibida = notification; });
        
        var notification = new UserNotification(NotificationType.Update, usuario, DateTime.Now);
        
        _service.Notify(notification);
        
        recibida.Should().NotBeNull();
        recibida!.Type.Should().Be(NotificationType.Update);
        recibida.User.Should().BeEquivalentTo(usuario);
    }

    [Test]
    public void Notify_NotificacionDelete()
    {
        UserNotification? recibida = null;
        var usuario = CrearUsuario(1);
        
        _service.Notifications.Subscribe(notification => { recibida = notification; });
        
        var notification = new UserNotification(NotificationType.Delete, usuario, DateTime.Now);

        _service.Notify(notification);

        recibida.Should().NotBeNull();
        recibida!.Type.Should().Be(NotificationType.Delete);
        recibida.User.Should().BeEquivalentTo(usuario);
    }

    private static User CrearUsuario(int id)
    {
        return new User(
            id,
            "Jesus",
            "Jcobo",
            "jesus@email.com",
            new Address(
                "Calle Mayor",
                "1A",
                "Madrid",
                "28001",
                new Geo(
                    "40.4168",
                    "-3.7038"
                )
            ),
            "600123123",
            "jesusweb.com",
            new Company(
                "jesusDev",
                "Software",
                "web development"
            )
        );
    }
}
