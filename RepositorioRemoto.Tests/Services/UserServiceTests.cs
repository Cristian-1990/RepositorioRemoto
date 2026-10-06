using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RepositorioRemoto.Cache;
using RepositorioRemoto.Config;
using RepositorioRemoto.Dto;
using RepositorioRemoto.Errors;
using RepositorioRemoto.Models;
using RepositorioRemoto.Notifications;
using RepositorioRemoto.Repositories;
using RepositorioRemoto.Services;
using RepositorioRemoto.Validators;

namespace RepositorioRemoto.Tests.Services;

/// <summary>
/// Tests unitarios de UserService. Todas las dependencias van simuladas con Moq.
/// </summary>
[TestFixture]
public class UserServiceTests
{
    private Mock<IUserRepository> _mockLocal = null!;
    private Mock<IUserRemoteRepository> _mockRemote = null!;
    private Mock<ICacheService> _mockCache = null!;
    private Mock<IUserValidator> _mockValidator = null!;
    private Mock<INotificationService> _mockNotifications = null!;
    private string _carpetaExport = null!;
    private UserService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLocal = new Mock<IUserRepository>();
        _mockRemote = new Mock<IUserRemoteRepository>();
        _mockCache = new Mock<ICacheService>();
        _mockValidator = new Mock<IUserValidator>();
        _mockNotifications = new Mock<INotificationService>();

        _carpetaExport = Path.Combine(Path.GetTempPath(), $"RepoRemotoTests_{Guid.NewGuid():N}");

        var exportConfig = Options.Create(new ExportConfig
        {
            OutputDirectory = _carpetaExport,
            FileName = "usuarios.json"
        });

        _service = new UserService(
            _mockLocal.Object,
            _mockRemote.Object,
            _mockCache.Object,
            _mockValidator.Object,
            _mockNotifications.Object,
            exportConfig,
            new Mock<ILogger<UserService>>().Object);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_carpetaExport))
        {
            Directory.Delete(_carpetaExport, true);
        }
    }

    /// <summary>Usuario de prueba con datos propios.</summary>
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

    private static AddressDto CrearAddressDto() =>
        new("Calle Mayor", "2B", "Leganés", "28911",
            new GeoDto("40.3272", "-3.7635"));

    private static CompanyDto CrearCompanyDto() =>
        new("Cafetería Nero", "El mejor café de Leganés", "coffee-shop");

    private static CreateUserRequest CrearCreateRequest() =>
        new("Cristian", "crisalvfer", "cristian@gmail.com",
            CrearAddressDto(), "600123456", "cafeterianero.es", CrearCompanyDto());

    private static UpdateUserRequest CrearUpdateRequest(int id) =>
        new(id, "Cristian", "crisalvfer", "cristian@gmail.com",
            CrearAddressDto(), "600123456", "cafeterianero.es", CrearCompanyDto());
    [TestFixture]
    public class CasosPositivos : UserServiceTests
    {
        [Test]
        public async Task GetAllAsync_ConDatosEnLocal_NoDeberiaLlamarALaApi()
        {
            // Arrange
            _mockLocal.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<User> { CrearUser(1, "Cristian") });

            // Act
            var resultado = await _service.GetAllAsync();

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().HaveCount(1);
            _mockRemote.Verify(r => r.GetAllAsync(), Times.Never);
        }
        
        [Test]
        public async Task UpdateAsync_NoEstabaEnLocal_DeberiaActualizarLaCacheYNotificar()
        {
            // Arrange
            var request = CrearUpdateRequest(1);
            var actualizado = CrearUser(1, "Cristian Alvarez");
            _mockValidator.Setup(v => v.ValidateUpdate(request))
                .Returns(Result.Success<UpdateUserRequest, DomainError>(request));
            _mockRemote.Setup(r => r.UpdateAsync(1, request))
                .ReturnsAsync(Result.Success<User, DomainError>(actualizado));
            _mockLocal.Setup(r => r.UpdateAsync(actualizado)).ReturnsAsync((User?)null);

            // Act
            var resultado = await _service.UpdateAsync(1, request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            _mockCache.Verify(c => c.SetAsync("user:1", It.IsAny<User>(), null), Times.Once);
            _mockNotifications.Verify(n => n.NotifyUpdated(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_NoEstabaEnLocal_DeberiaBorrarDeCacheYNotificar()
        {
            // Arrange
            _mockRemote.Setup(r => r.DeleteAsync(1))
                .ReturnsAsync(Result.Success<bool, DomainError>(true));
            _mockLocal.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

            // Act
            var resultado = await _service.DeleteAsync(1);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            _mockCache.Verify(c => c.RemoveAsync("user:1"), Times.Once);
            _mockNotifications.Verify(n => n.NotifyDeleted(1), Times.Once);
        }

        [Test]
        public async Task GetAllAsync_SinDatosEnLocal_DeberiaCargarDesdeLaApi()
        {
            // Arrange
            _mockLocal.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());
            _mockRemote.Setup(r => r.GetAllAsync())
                .ReturnsAsync(Result.Success<List<User>, DomainError>(
                    new List<User> { CrearUser(1, "Cristian"), CrearUser(2, "Jesus") }));

            // Act
            var resultado = await _service.GetAllAsync();

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().HaveCount(2);
            _mockLocal.Verify(r => r.CreateAllAsync(It.IsAny<IEnumerable<User>>()), Times.Once);
        }

        [Test]
        public async Task GetByIdAsync_EnCache_NoDeberiaTocarLaBaseDeDatos()
        {
            // Arrange
            _mockCache.Setup(c => c.GetAsync<User>("user:1"))
                .ReturnsAsync(CrearUser(1, "Cristian"));

            // Act
            var resultado = await _service.GetByIdAsync(1);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            _mockLocal.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockRemote.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task GetByIdAsync_EnBaseDeDatos_DeberiaGuardarloEnCache()
        {
            // Arrange
            _mockCache.Setup(c => c.GetAsync<User>("user:1")).ReturnsAsync((User?)null);
            _mockLocal.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CrearUser(1, "Cristian"));

            // Act
            var resultado = await _service.GetByIdAsync(1);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            _mockCache.Verify(c => c.SetAsync("user:1", It.IsAny<User>(), null), Times.Once);
            _mockRemote.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task GetByIdAsync_SoloEnLaApi_DeberiaGuardarloEnBaseDeDatosYCache()
        {
            // Arrange
            _mockCache.Setup(c => c.GetAsync<User>("user:5")).ReturnsAsync((User?)null);
            _mockLocal.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((User?)null);
            _mockRemote.Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(Result.Success<User, DomainError>(CrearUser(5, "Cristian")));

            // Act
            var resultado = await _service.GetByIdAsync(5);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            _mockLocal.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
            _mockCache.Verify(c => c.SetAsync("user:5", It.IsAny<User>(), null), Times.Once);
        }

        [Test]
        public async Task CreateAsync_DatosValidos_DeberiaNotificarLaCreacion()
        {
            // Arrange
            var request = CrearCreateRequest();
            _mockValidator.Setup(v => v.ValidateCreate(request))
                .Returns(Result.Success<CreateUserRequest, DomainError>(request));
            _mockRemote.Setup(r => r.CreateAsync(request))
                .ReturnsAsync(Result.Success<User, DomainError>(CrearUser(11, "Cristian")));
            _mockLocal.Setup(r => r.GetByIdAsync(11)).ReturnsAsync((User?)null);

            // Act
            var resultado = await _service.CreateAsync(request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Id.Should().Be(11);
            _mockNotifications.Verify(n => n.NotifyCreated(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_DatosValidos_DeberiaNotificarLaActualizacion()
        {
            // Arrange
            var request = CrearUpdateRequest(1);
            var actualizado = CrearUser(1, "Cristian Alvarez");
            _mockValidator.Setup(v => v.ValidateUpdate(request))
                .Returns(Result.Success<UpdateUserRequest, DomainError>(request));
            _mockRemote.Setup(r => r.UpdateAsync(1, request))
                .ReturnsAsync(Result.Success<User, DomainError>(actualizado));
            _mockLocal.Setup(r => r.UpdateAsync(actualizado)).ReturnsAsync(actualizado);

            // Act
            var resultado = await _service.UpdateAsync(1, request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            _mockNotifications.Verify(n => n.NotifyUpdated(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_Existente_DeberiaBorrarDeCacheYNotificar()
        {
            // Arrange
            _mockRemote.Setup(r => r.DeleteAsync(1))
                .ReturnsAsync(Result.Success<bool, DomainError>(true));
            _mockLocal.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var resultado = await _service.DeleteAsync(1);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            _mockCache.Verify(c => c.RemoveAsync("user:1"), Times.Once);
            _mockNotifications.Verify(n => n.NotifyDeleted(1), Times.Once);
        }

        [Test]
        public async Task ExportToJsonAsync_DeberiaCrearElFichero()
        {
            // Arrange
            _mockLocal.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<User> { CrearUser(1, "Cristian") });

            // Act
            var resultado = await _service.ExportToJsonAsync();

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            File.Exists(resultado.Value).Should().BeTrue();
            var contenido = await File.ReadAllTextAsync(resultado.Value);
            contenido.Should().Contain("Cristian");
        }
    }
    
    [TestFixture]
    public class CasosNegativos : UserServiceTests
    {
        [Test]
        public async Task CreateAsync_ApiFalla_NoDeberiaGuardarNiNotificar()
        {
            // Arrange
            var request = CrearCreateRequest();
            _mockValidator.Setup(v => v.ValidateCreate(request))
                .Returns(Result.Success<CreateUserRequest, DomainError>(request));
            _mockRemote.Setup(r => r.CreateAsync(request))
                .ReturnsAsync(Result.Failure<User, DomainError>(
                    DomainErrors.ApiError(500, "Error del servidor")));

            // Act
            var resultado = await _service.CreateAsync(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.ApiError>();
            _mockLocal.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
            _mockNotifications.Verify(n => n.NotifyCreated(It.IsAny<User>()), Times.Never);
        }
        
                [Test]
        public async Task GetAllAsync_SinDatosEnLocalYApiFalla_DeberiaDevolverElError()
        {
            // Arrange
            _mockLocal.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());
            _mockRemote.Setup(r => r.GetAllAsync())
                .ReturnsAsync(Result.Failure<List<User>, DomainError>(
                    DomainErrors.ApiError(500, "Error del servidor")));

            // Act
            var resultado = await _service.GetAllAsync();

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.ApiError>();
            _mockLocal.Verify(r => r.CreateAllAsync(It.IsAny<IEnumerable<User>>()), Times.Never);
        }

        [Test]
        public async Task UpdateAsync_DatosInvalidos_NoDeberiaLlamarALaApi()
        {
            // Arrange
            var request = CrearUpdateRequest(1);
            _mockValidator.Setup(v => v.ValidateUpdate(request))
                .Returns(Result.Failure<UpdateUserRequest, DomainError>(
                    DomainErrors.Validation(new List<string> { "El nombre es obligatorio" })));

            // Act
            var resultado = await _service.UpdateAsync(1, request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.Validation>();
            _mockRemote.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateUserRequest>()), Times.Never);
            _mockNotifications.Verify(n => n.NotifyUpdated(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public async Task UpdateAsync_ApiFalla_NoDeberiaNotificar()
        {
            // Arrange
            var request = CrearUpdateRequest(1);
            _mockValidator.Setup(v => v.ValidateUpdate(request))
                .Returns(Result.Success<UpdateUserRequest, DomainError>(request));
            _mockRemote.Setup(r => r.UpdateAsync(1, request))
                .ReturnsAsync(Result.Failure<User, DomainError>(
                    DomainErrors.ApiError(500, "Error del servidor")));

            // Act
            var resultado = await _service.UpdateAsync(1, request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.ApiError>();
            _mockLocal.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
            _mockNotifications.Verify(n => n.NotifyUpdated(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public async Task ExportToJsonAsync_SiFallaLaCarga_DeberiaDevolverElError()
        {
            // Arrange
            _mockLocal.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());
            _mockRemote.Setup(r => r.GetAllAsync())
                .ReturnsAsync(Result.Failure<List<User>, DomainError>(
                    DomainErrors.ApiError(500, "Error del servidor")));

            // Act
            var resultado = await _service.ExportToJsonAsync();

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.ApiError>();
            Directory.Exists(_carpetaExport).Should().BeFalse();
        }
        
        [Test]
        public async Task CreateAsync_DatosInvalidos_NoDeberiaLlamarALaApi()
        {
            // Arrange
            var request = CrearCreateRequest();
            _mockValidator.Setup(v => v.ValidateCreate(request))
                .Returns(Result.Failure<CreateUserRequest, DomainError>(
                    DomainErrors.Validation(new List<string> { "El nombre es obligatorio" })));

            // Act
            var resultado = await _service.CreateAsync(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.Validation>();
            _mockRemote.Verify(r => r.CreateAsync(It.IsAny<CreateUserRequest>()), Times.Never);
            _mockNotifications.Verify(n => n.NotifyCreated(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public async Task CreateAsync_IdYaExistenteEnLocal_DeberiaDevolverAlreadyExists()
        {
            // Arrange
            var request = CrearCreateRequest();
            _mockValidator.Setup(v => v.ValidateCreate(request))
                .Returns(Result.Success<CreateUserRequest, DomainError>(request));
            _mockRemote.Setup(r => r.CreateAsync(request))
                .ReturnsAsync(Result.Success<User, DomainError>(CrearUser(11, "Cristian")));
            _mockLocal.Setup(r => r.GetByIdAsync(11)).ReturnsAsync(CrearUser(11, "Otro"));

            // Act
            var resultado = await _service.CreateAsync(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.AlreadyExists>();
            _mockLocal.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public async Task GetByIdAsync_NoExisteEnNingunSitio_DeberiaDevolverNotFound()
        {
            // Arrange
            _mockCache.Setup(c => c.GetAsync<User>("user:999")).ReturnsAsync((User?)null);
            _mockLocal.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);
            _mockRemote.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync(Result.Failure<User, DomainError>(DomainErrors.NotFound(999)));

            // Act
            var resultado = await _service.GetByIdAsync(999);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.NotFound>();
            _mockLocal.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public async Task DeleteAsync_NoExisteEnLaApi_NoDeberiaNotificar()
        {
            // Arrange
            _mockRemote.Setup(r => r.DeleteAsync(999))
                .ReturnsAsync(Result.Failure<bool, DomainError>(DomainErrors.NotFound(999)));

            // Act
            var resultado = await _service.DeleteAsync(999);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.NotFound>();
            _mockLocal.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
            _mockNotifications.Verify(n => n.NotifyDeleted(It.IsAny<int>()), Times.Never);
        }
    }
}