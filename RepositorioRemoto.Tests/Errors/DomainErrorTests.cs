using FluentAssertions;
using NUnit.Framework;
using RepositorioRemoto.Errors;

namespace RepositorioRemoto.Tests.Errors;

/// <summary>
/// Tests de DomainError
/// </summary>
[TestFixture]
public class DomainErrorTests
{
    [Test]
    public void NotFound_DeberiaIncluirElIdEnElMensaje()
    {
        // Act
        var error = DomainErrors.NotFound(5);

        // Assert
        error.Should().BeOfType<DomainError.NotFound>();
        error.Message.Should().Contain("5");
    }

    [Test]
    public void Validation_DeberiaUnirTodosLosErrores()
    {
        // Act
        var error = DomainErrors.Validation(["Nombre requerido", "Email inválido"]);

        // Assert
        error.Should().BeOfType<DomainError.Validation>();
        error.Message.Should().Be("Nombre requerido, Email inválido");
    }

    [Test]
    public void AlreadyExists_DeberiaIncluirElIdEnElMensaje()
    {
        // Act
        var error = DomainErrors.AlreadyExists(11);

        // Assert
        error.Should().BeOfType<DomainError.AlreadyExists>();
        error.Message.Should().Contain("11");
    }

    [Test]
    public void ApiError_DeberiaIncluirCodigoYDetalle()
    {
        // Act
        var error = DomainErrors.ApiError(404, "Not Found");

        // Assert
        error.Should().BeOfType<DomainError.ApiError>();
        error.Message.Should().Contain("404").And.Contain("Not Found");
    }

    [Test]
    public void Storage_DeberiaIncluirElMensajeDeLaExcepcion()
    {
        // Act
        var error = DomainErrors.Storage(new InvalidOperationException("disco lleno"));

        // Assert
        error.Should().BeOfType<DomainError.Storage>();
        error.Message.Should().Contain("disco lleno");
    }
}