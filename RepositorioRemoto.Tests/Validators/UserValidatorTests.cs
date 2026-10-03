using FluentAssertions;
using NUnit.Framework;
using RepositorioRemoto.Dto;
using RepositorioRemoto.Errors;
using RepositorioRemoto.Validators;

namespace RepositorioRemoto.Tests.Validators;

/// <summary>
/// Tests de UserValidator.
/// </summary>
[TestFixture]
public class UserValidatorTests
{
    private UserValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new UserValidator();
    }

    private static AddressDto Direccion() =>
        new("Gran Vía", "3º B", "Madrid", "28013", new GeoDto("40.4200", "-3.7050"));

    private static CompanyDto Empresa() =>
        new("Cafetería Nero", "El mejor café de Leganés", "cafés de especialidad");

    private static CreateUserRequest PeticionCrear(
        string name = "Cristian",
        string username = "cristian",
        string email = "cristian@gmail.com") =>
        new(name, username, email, Direccion(), "600123456", "cristian.dev", Empresa());

    private static UpdateUserRequest PeticionActualizar(
        int id = 1,
        string name = "Cristian",
        string username = "cristian",
        string email = "cristian@gmail.com") =>
        new(id, name, username, email, Direccion(), "600123456", "cristian.dev", Empresa());

    [TestFixture]
    public class CasosPositivos : UserValidatorTests
    {
        [Test]
        public void ValidateCreate_DatosValidos_DeberiaRetornarSuccess()
        {
            // Arrange
            var request = PeticionCrear();

            // Act
            var resultado = _validator.ValidateCreate(request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().Be(request);
        }

        [Test]
        public void ValidateUpdate_DatosValidos_DeberiaRetornarSuccess()
        {
            // Arrange
            var request = PeticionActualizar();

            // Act
            var resultado = _validator.ValidateUpdate(request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().Be(request);
        }
    }

    [TestFixture]
    public class CasosNegativos : UserValidatorTests
    {
        [Test]
        public void ValidateCreate_NombreVacio_DeberiaRetornarFailure()
        {
            // Arrange
            var request = PeticionCrear(name: "   ");

            // Act
            var resultado = _validator.ValidateCreate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.Validation>();
            resultado.Error.Message.Should().Contain("nombre es obligatorio");
        }

        [Test]
        public void ValidateCreate_UsuarioVacio_DeberiaRetornarFailure()
        {
            // Arrange
            var request = PeticionCrear(username: "");

            // Act
            var resultado = _validator.ValidateCreate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Message.Should().Contain("nombre de usuario es obligatorio");
        }

        [Test]
        public void ValidateCreate_EmailVacio_DeberiaRetornarFailure()
        {
            // Arrange
            var request = PeticionCrear(email: "");

            // Act
            var resultado = _validator.ValidateCreate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Message.Should().Contain("email es obligatorio");
        }

        [Test]
        public void ValidateCreate_EmailSinArroba_DeberiaRetornarFailure()
        {
            // Arrange
            var request = PeticionCrear(email: "cristian.gmail.com");

            // Act
            var resultado = _validator.ValidateCreate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Message.Should().Contain("formato válido");
        }

        [Test]
        public void ValidateCreate_VariosCamposMal_DeberiaDevolverTodosLosErrores()
        {
            // Arrange
            var request = PeticionCrear(name: "", username: "", email: "");

            // Act
            var resultado = _validator.ValidateCreate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Message.Should()
                .Contain("nombre es obligatorio").And
                .Contain("nombre de usuario es obligatorio").And
                .Contain("email es obligatorio");
        }

        [Test]
        public void ValidateUpdate_IdNoPositivo_DeberiaRetornarFailure()
        {
            // Arrange
            var request = PeticionActualizar(id: 0);

            // Act
            var resultado = _validator.ValidateUpdate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Message.Should().Contain("identificador");
        }

        [Test]
        public void ValidateUpdate_IdNoPositivoYNombreVacio_DeberiaDevolverLosDosErrores()
        {
            // Arrange
            var request = PeticionActualizar(id: -5, name: "");

            // Act
            var resultado = _validator.ValidateUpdate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Message.Should()
                .Contain("nombre es obligatorio").And
                .Contain("identificador");
        }
    }
}