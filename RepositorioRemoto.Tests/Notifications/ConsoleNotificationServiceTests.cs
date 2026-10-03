using System.Reactive.Linq;
using FluentAssertions;
using NUnit.Framework;
using RepositorioRemoto.Models;
using RepositorioRemoto.Notifications;

namespace RepositorioRemoto.Tests.Notifications;

/// <summary>
/// Tests unitarios del servicio de notificaciones reactivo.
/// </summary>
[TestFixture]
public class ConsoleNotificationServiceTests
{
    private ConsoleNotificationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new ConsoleNotificationService();
    }

    [TearDown]
    public void TearDown()
    {
        _service.Dispose();
    }

    /// <summary>Construye un usuario de prueba con datos propios.</summary>
    private static User CrearUser(int id, string name) =>
        new(
            id,
            name,
            "crisalvfer",
            "cristian@gmail.com",
            new Address("Calle Mayor", "2B", "Leganés", "28911",
                new Geo("40.3272", "-3.7635")),
            "600123456",
            "cafeterianero.es",
            new Company("Cafetería Nero", "El mejor café de Leganés", "coffee-shop")
        );

    [TestFixture]
    public class CasosPositivos : ConsoleNotificationServiceTests
    {
        [Test]
        public void NotifyCreated_DeberiaEmitirEventoCreate()
        {
            // Arrange
            var recibidas = new List<Notification>();
            _service.Notifications.Subscribe(n => recibidas.Add(n));

            // Act
            _service.NotifyCreated(CrearUser(1, "Cristian"));

            // Assert
            recibidas.Should().HaveCount(1);
            recibidas[0].Type.Should().Be(NotificationType.Create);
            recibidas[0].Message.Should().Contain("Cristian");
        }

        [Test]
        public void NotifyUpdated_DeberiaEmitirEventoUpdate()
        {
            // Arrange
            var recibidas = new List<Notification>();
            _service.Notifications.Subscribe(n => recibidas.Add(n));

            // Act
            _service.NotifyUpdated(CrearUser(1, "Cristian"));

            // Assert
            recibidas.Should().HaveCount(1);
            recibidas[0].Type.Should().Be(NotificationType.Update);
        }

        [Test]
        public void NotifyDeleted_DeberiaEmitirEventoDelete()
        {
            // Arrange
            var recibidas = new List<Notification>();
            _service.Notifications.Subscribe(n => recibidas.Add(n));

            // Act
            _service.NotifyDeleted(7);

            // Assert
            recibidas.Should().HaveCount(1);
            recibidas[0].Type.Should().Be(NotificationType.Delete);
            recibidas[0].Message.Should().Contain("7");
        }

        [Test]
        public void MultiplesSuscriptores_DeberianRecibirLaMismaNotificacion()
        {
            // Arrange
            int primero = 0, segundo = 0, tercero = 0;
            _service.Notifications.Subscribe(_ => primero++);
            _service.Notifications.Subscribe(_ => segundo++);
            _service.Notifications.Subscribe(_ => tercero++);

            // Act
            _service.NotifyCreated(CrearUser(1, "Cristian"));

            // Assert
            primero.Should().Be(1);
            segundo.Should().Be(1);
            tercero.Should().Be(1);
        }
    }

    [TestFixture]
    public class CasosNegativos : ConsoleNotificationServiceTests
    {
        [Test]
        public void SuscribirseDespuesDelEvento_NoDeberiaRecibirlo()
        {
            // Arrange: el evento ocurre ANTES de suscribirse
            _service.NotifyCreated(CrearUser(1, "Cristian"));

            var recibidas = new List<Notification>();

            // Act
            _service.Notifications.Subscribe(n => recibidas.Add(n));

            // Assert: es un observable CALIENTE, lo anterior se ha perdido
            recibidas.Should().BeEmpty();
        }
    }
}