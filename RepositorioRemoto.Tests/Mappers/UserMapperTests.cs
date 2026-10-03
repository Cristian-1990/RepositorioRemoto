using FluentAssertions;
using NUnit.Framework;
using RepositorioRemoto.Dto;
using RepositorioRemoto.Mappers;
using RepositorioRemoto.Models;

namespace RepositorioRemoto.Tests.Mappers;

/// <summary>
/// Tests de UserMapper
/// </summary>
[TestFixture]
public class UserMapperTests
{
    private static User CrearUsuario() => new(
        1, "Leanne Graham", "Bret", "Sincere@april.biz",
        new Address("Kulas Light", "Apt. 556", "Gwenborough", "92998-3874", new Geo("-37.3159", "81.1496")),
        "1-770-736-8031 x56442", "hildegard.org",
        new Company("Romaguera-Crona", "Multi-layered client-server neural-net", "harness real-time e-markets"));

    private static JsonPlaceholderUserDto CrearDto() => new(
        1, "Leanne Graham", "Bret", "Sincere@april.biz",
        new AddressDto("Kulas Light", "Apt. 556", "Gwenborough", "92998-3874", new GeoDto("-37.3159", "81.1496")),
        "1-770-736-8031 x56442", "hildegard.org",
        new CompanyDto("Romaguera-Crona", "Multi-layered client-server neural-net", "harness real-time e-markets"));

    [Test]
    public void ToUser_DesdeDtoDeLaApi_DeberiaConservarTodosLosCampos()
    {
        // Arrange
        var dto = CrearDto();

        // Act
        var user = dto.ToUser();

        // Assert
        user.Should().Be(CrearUsuario());
    }

    [Test]
    public void ToResponse_DeberiaConservarTodosLosCampos()
    {
        // Arrange
        var user = CrearUsuario();

        // Act
        var response = user.ToResponse();

        // Assert
        response.Should().BeEquivalentTo(user);
    }

    [Test]
    public void ToCreateRequest_DeberiaConservarTodosLosCamposMenosElId()
    {
        // Arrange
        var user = CrearUsuario();

        // Act
        var request = user.ToCreateRequest();

        // Assert
        request.Should().BeEquivalentTo(user, options => options.ExcludingMissingMembers());
    }

    [Test]
    public void ToUpdateRequest_DeberiaConservarTodosLosCampos()
    {
        // Arrange
        var user = CrearUsuario();

        // Act
        var request = user.ToUpdateRequest();

        // Assert
        request.Should().BeEquivalentTo(user);
    }

    [Test]
    public void ToUser_DesdeCreateRequest_DeberiaUsarElIdRecibido()
    {
        // Arrange
        var request = CrearUsuario().ToCreateRequest();

        // Act
        var user = request.ToUser(11);

        // Assert
        user.Should().Be(CrearUsuario() with { Id = 11 });
    }

    [Test]
    public void ToUser_DesdeUpdateRequest_DeberiaConservarTodosLosCampos()
    {
        // Arrange
        var request = CrearUsuario().ToUpdateRequest();

        // Act
        var user = request.ToUser();

        // Assert
        user.Should().Be(CrearUsuario());
    }
}