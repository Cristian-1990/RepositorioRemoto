using System.Net;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Refit;
using RepositorioRemoto.Api;
using RepositorioRemoto.Dto;
using RepositorioRemoto.Errors;
using RepositorioRemoto.Repositories;
 
namespace RepositorioRemoto.Tests.Repositories;
 
/// <summary>
/// Tests unitarios de UserRemoteRepository con Moq para IJsonPlaceholderApi.
/// </summary>
[TestFixture]
public class UserRemoteRepositoryTests
{
    private Mock<IJsonPlaceholderApi> _mockApi = null!;
    private UserRemoteRepository _repository = null!;
 
    [SetUp]
    public void SetUp()
    {
        _mockApi = new Mock<IJsonPlaceholderApi>();
        _repository = new UserRemoteRepository(_mockApi.Object);
    }
 
    /// <summary>Construye un DTO de prueba con datos propios.</summary>
    private static JsonPlaceholderUserDto CrearDto(int id, string name) =>
        new(
            id,
            name,
            "crisalvfer",
            "cristian@gmail.com",
            new AddressDto("Calle Mayor", "2B", "Leganés", "28911",
                new GeoDto("40.3272", "-3.7635")),
            "600123456",
            "cafeterianero.es",
            new CompanyDto("Cafetería Nero", "El mejor café de Leganés", "coffee-shop")
        );
 
    /// <summary>Crea una ApiException real de Refit con el código HTTP indicado.</summary>
    private static async Task<ApiException> CrearApiException(HttpStatusCode status)
    {
        var peticion = new HttpRequestMessage(HttpMethod.Get, "https://jsonplaceholder.typicode.com/users");
        var respuesta = new HttpResponseMessage(status);
        return await ApiException.Create(peticion, HttpMethod.Get, respuesta, new RefitSettings());
    }
 
    [TestFixture]
    public class CasosPositivos : UserRemoteRepositoryTests
    {
        [Test]
        public async Task GetAllAsync_DeberiaRetornarTodosLosUsuarios()
        {
            // Arrange
            var dtos = new List<JsonPlaceholderUserDto>
            {
                CrearDto(1, "Cristian"),
                CrearDto(2, "Jesús")
            };
            _mockApi.Setup(a => a.GetUsersAsync()).ReturnsAsync(dtos);
 
            // Act
            var resultado = await _repository.GetAllAsync();
 
            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().HaveCount(2);
            resultado.Value[0].Name.Should().Be("Cristian");
        }
 
        [Test]
        public async Task GetByIdAsync_Existente_DeberiaRetornarSuccess()
        {
            // Arrange
            _mockApi.Setup(a => a.GetUserByIdAsync(1)).ReturnsAsync(CrearDto(1, "Cristian"));
 
            // Act
            var resultado = await _repository.GetByIdAsync(1);
 
            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Name.Should().Be("Cristian");
        }
 
        [Test]
        public async Task CreateAsync_DatosValidos_DeberiaRetornarSuccess()
        {
            // Arrange
            var dto = CrearDto(11, "Cristian");
            var request = new CreateUserRequest(
                dto.Name, dto.Username, dto.Email,
                dto.Address, dto.Phone, dto.Website, dto.Company);
            _mockApi.Setup(a => a.CreateUserAsync(request)).ReturnsAsync(dto);
 
            // Act
            var resultado = await _repository.CreateAsync(request);
 
            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Id.Should().Be(11);
        }
 
        [Test]
        public async Task UpdateAsync_Existente_DeberiaRetornarSuccess()
        {
            // Arrange
            var dto = CrearDto(1, "Cristian Álvarez");
            var request = new UpdateUserRequest(
                dto.Id, dto.Name, dto.Username, dto.Email,
                dto.Address, dto.Phone, dto.Website, dto.Company);
            _mockApi.Setup(a => a.UpdateUserAsync(1, request)).ReturnsAsync(dto);
 
            // Act
            var resultado = await _repository.UpdateAsync(1, request);
 
            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Name.Should().Be("Cristian Álvarez");
        }
 
        [Test]
        public async Task DeleteAsync_Existente_DeberiaRetornarSuccess()
        {
            // Arrange
            _mockApi.Setup(a => a.DeleteUserAsync(1)).Returns(Task.CompletedTask);
 
            // Act
            var resultado = await _repository.DeleteAsync(1);
 
            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().BeTrue();
        }
    }
 
    [TestFixture]
    public class CasosNegativos : UserRemoteRepositoryTests
    {
        [Test]
        public async Task GetByIdAsync_Inexistente_DeberiaRetornarNotFound()
        {
            // Arrange
            _mockApi.Setup(a => a.GetUserByIdAsync(999))
                .ReturnsAsync((JsonPlaceholderUserDto?)null);
 
            // Act
            var resultado = await _repository.GetByIdAsync(999);
 
            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.NotFound>();
        }
 
        [Test]
        public async Task GetAllAsync_ApiFalla_DeberiaRetornarApiError()
        {
            // Arrange
            var excepcion = await CrearApiException(HttpStatusCode.InternalServerError);
            _mockApi.Setup(a => a.GetUsersAsync()).ThrowsAsync(excepcion);
 
            // Act
            var resultado = await _repository.GetAllAsync();
 
            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.ApiError>()
                .Which.StatusCode.Should().Be(500);
        }
 
        [Test]
        public async Task GetByIdAsync_Api404_DeberiaRetornarNotFound()
        {
            // Arrange
            var excepcion = await CrearApiException(HttpStatusCode.NotFound);
            _mockApi.Setup(a => a.GetUserByIdAsync(999)).ThrowsAsync(excepcion);
 
            // Act
            var resultado = await _repository.GetByIdAsync(999);
 
            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.NotFound>()
                .Which.Id.Should().Be(999);
        }
 
        [Test]
        public async Task GetByIdAsync_ApiFalla_DeberiaRetornarApiError()
        {
            // Arrange
            var excepcion = await CrearApiException(HttpStatusCode.InternalServerError);
            _mockApi.Setup(a => a.GetUserByIdAsync(1)).ThrowsAsync(excepcion);
 
            // Act
            var resultado = await _repository.GetByIdAsync(1);
 
            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.ApiError>()
                .Which.StatusCode.Should().Be(500);
        }
 
        [Test]
        public async Task CreateAsync_ApiFalla_DeberiaRetornarApiError()
        {
            // Arrange
            var dto = CrearDto(11, "Cristian");
            var request = new CreateUserRequest(
                dto.Name, dto.Username, dto.Email,
                dto.Address, dto.Phone, dto.Website, dto.Company);
            var excepcion = await CrearApiException(HttpStatusCode.InternalServerError);
            _mockApi.Setup(a => a.CreateUserAsync(request)).ThrowsAsync(excepcion);
 
            // Act
            var resultado = await _repository.CreateAsync(request);
 
            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.ApiError>()
                .Which.StatusCode.Should().Be(500);
        }
 
        [Test]
        public async Task UpdateAsync_Api404_DeberiaRetornarNotFound()
        {
            // Arrange
            var dto = CrearDto(999, "Cristian");
            var request = new UpdateUserRequest(
                dto.Id, dto.Name, dto.Username, dto.Email,
                dto.Address, dto.Phone, dto.Website, dto.Company);
            var excepcion = await CrearApiException(HttpStatusCode.NotFound);
            _mockApi.Setup(a => a.UpdateUserAsync(999, request)).ThrowsAsync(excepcion);
 
            // Act
            var resultado = await _repository.UpdateAsync(999, request);
 
            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.NotFound>()
                .Which.Id.Should().Be(999);
        }
 
        [Test]
        public async Task UpdateAsync_ApiFalla_DeberiaRetornarApiError()
        {
            // Arrange
            var dto = CrearDto(1, "Cristian");
            var request = new UpdateUserRequest(
                dto.Id, dto.Name, dto.Username, dto.Email,
                dto.Address, dto.Phone, dto.Website, dto.Company);
            var excepcion = await CrearApiException(HttpStatusCode.InternalServerError);
            _mockApi.Setup(a => a.UpdateUserAsync(1, request)).ThrowsAsync(excepcion);
 
            // Act
            var resultado = await _repository.UpdateAsync(1, request);
 
            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.ApiError>()
                .Which.StatusCode.Should().Be(500);
        }
 
        [Test]
        public async Task DeleteAsync_Api404_DeberiaRetornarNotFound()
        {
            // Arrange
            var excepcion = await CrearApiException(HttpStatusCode.NotFound);
            _mockApi.Setup(a => a.DeleteUserAsync(999)).ThrowsAsync(excepcion);
 
            // Act
            var resultado = await _repository.DeleteAsync(999);
 
            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.NotFound>()
                .Which.Id.Should().Be(999);
        }
 
        [Test]
        public async Task DeleteAsync_ApiFalla_DeberiaRetornarApiError()
        {
            // Arrange
            var excepcion = await CrearApiException(HttpStatusCode.InternalServerError);
            _mockApi.Setup(a => a.DeleteUserAsync(1)).ThrowsAsync(excepcion);
 
            // Act
            var resultado = await _repository.DeleteAsync(1);
 
            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.ApiError>()
                .Which.StatusCode.Should().Be(500);
        }
    }
}
 
