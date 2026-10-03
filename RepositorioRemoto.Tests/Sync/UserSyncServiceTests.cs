using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RepositorioRemoto.Cache;
using RepositorioRemoto.Errors;
using RepositorioRemoto.Models;
using RepositorioRemoto.Repositories;
using RepositorioRemoto.Sync;

namespace RepositorioRemoto.Tests.Sync;

/// <summary>
/// Tests de UserSyncService con mocks de los dos repositorios y la caché.
/// No tocan ni la API ni la base de datos.
/// </summary>
[TestFixture]
public class UserSyncServiceTests
{
    private Mock<IUserRemoteRepository> _remoto = null!;
    private Mock<IUserRepository> _local = null!;
    private Mock<ICacheService> _cache = null!;
    private UserSyncService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _remoto = new Mock<IUserRemoteRepository>();
        _local = new Mock<IUserRepository>();
        _cache = new Mock<ICacheService>();

        _service = new UserSyncService(
            _remoto.Object,
            _local.Object,
            _cache.Object,
            new Mock<ILogger<UserSyncService>>().Object);
    }

    private static User CrearUsuario(int id = 1) => new(
        id,
        "Cristian",
        "cristian",
        "cristian@gmail.com",
        new Address("Gran Vía", "3º B", "Madrid", "28013", new Geo("40.4200", "-3.7050")),
        "600123456",
        "cristian.dev",
        new Company("Cafetería Nero", "El mejor café de Leganés", "cafés de especialidad"));

    [TestFixture]
    public class CasosPositivos : UserSyncServiceTests
    {
        [Test]
        public async Task SyncAsync_ApiResponde_DeberiaDevolverTrue()
        {
            // Arrange
            var usuarios = new List<User> { CrearUsuario(1), CrearUsuario(2) };
            _remoto.Setup(r => r.GetAllAsync())
                   .ReturnsAsync(Result.Success<List<User>, DomainError>(usuarios));

            // Act
            var resultado = await _service.SyncAsync();

            // Assert
            resultado.Should().BeTrue();
        }

        [Test]
        public async Task SyncAsync_ApiResponde_DeberiaVaciarCacheYBaseDeDatos()
        {
            // Arrange
            _remoto.Setup(r => r.GetAllAsync())
                   .ReturnsAsync(Result.Success<List<User>, DomainError>([CrearUsuario()]));

            // Act
            await _service.SyncAsync();

            // Assert
            _cache.Verify(c => c.ClearAsync(), Times.Once);
            _local.Verify(r => r.DeleteAllAsync(), Times.Once);
        }

        [Test]
        public async Task SyncAsync_ApiResponde_DeberiaGuardarLosUsuariosRecibidos()
        {
            // Arrange
            var usuarios = new List<User> { CrearUsuario(1), CrearUsuario(2) };
            _remoto.Setup(r => r.GetAllAsync())
                   .ReturnsAsync(Result.Success<List<User>, DomainError>(usuarios));

            // Act
            await _service.SyncAsync();

            // Assert
            _local.Verify(r => r.CreateAllAsync(usuarios), Times.Once);
        }
    }

    [TestFixture]
    public class CasosNegativos : UserSyncServiceTests
    {
        [Test]
        public async Task SyncAsync_ApiFalla_DeberiaDevolverFalse()
        {
            // Arrange
            _remoto.Setup(r => r.GetAllAsync())
                   .ReturnsAsync(Result.Failure<List<User>, DomainError>(
                       DomainErrors.ApiError(503, "Service Unavailable")));

            // Act
            var resultado = await _service.SyncAsync();

            // Assert
            resultado.Should().BeFalse();
        }

        [Test]
        public async Task SyncAsync_ApiFalla_NoDeberiaGuardarNada()
        {
            // Arrange
            _remoto.Setup(r => r.GetAllAsync())
                   .ReturnsAsync(Result.Failure<List<User>, DomainError>(
                       DomainErrors.ApiError(503, "Service Unavailable")));

            // Act
            await _service.SyncAsync();

            // Assert
            _local.Verify(r => r.CreateAllAsync(It.IsAny<IEnumerable<User>>()), Times.Never);
        }
    }
}