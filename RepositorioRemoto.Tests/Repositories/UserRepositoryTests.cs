using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RepositorioRemoto.Entity;
using RepositorioRemoto.Models;
using RepositorioRemoto.Repositories;
using Testcontainers.PostgreSql;

namespace RepositorioRemoto.Tests.Repositories;

/// <summary>
/// Tests de integración de UserRepository con TestContainers.
/// </summary>
[TestFixture]
public class UserRepositoryTests
{
    private PostgreSqlContainer _container = null!;
    private AppDbContext _context = null!;
    private UserRepository _repository = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Un solo contenedor para todos los tests de la clase
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("test_db")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await _container.StartAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _container.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        // Base de datos limpia para cada test
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(_container.GetConnectionString());

        _context = new AppDbContext(optionsBuilder.Options);
        await _context.Database.EnsureCreatedAsync();

        _repository = new UserRepository(_context);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    private static User CrearUsuario(int id = 1, string name = "Cristian") => new(
        id,
        name,
        "cristian",
        "cristian@gmail.com",
        new Address("Gran Vía", "3º B", "Madrid", "28013", new Geo("40.4200", "-3.7050")),
        "600123456",
        "cristian.dev",
        new Company("Cafetería Nero", "El mejor café de Leganés", "cafés de especialidad"));

    [Test]
    public async Task CreateAsync_DeberiaGuardarElUsuario()
    {
        // Arrange
        var user = CrearUsuario();

        // Act
        var resultado = await _repository.CreateAsync(user);

        // Assert
        resultado.Should().Be(user);
        (await _repository.GetAllAsync()).Should().HaveCount(1);
    }

    [Test]
    public async Task CreateAsync_DeberiaRespetarElIdDeLaApi()
    {
        // Arrange
        var user = CrearUsuario(id: 7);

        // Act
        await _repository.CreateAsync(user);
        var guardado = await _repository.GetByIdAsync(7);

        // Assert
        guardado.Should().NotBeNull();
        guardado!.Id.Should().Be(7);
    }

    [Test]
    public async Task GetByIdAsync_Existente_DeberiaConservarLosDatosAnidados()
    {
        // Arrange
        var user = CrearUsuario();
        await _repository.CreateAsync(user);

        // Act
        var guardado = await _repository.GetByIdAsync(1);

        // Assert
        guardado.Should().Be(user);
        guardado!.Address.Geo.Lat.Should().Be("40.4200");
        guardado.Company.Name.Should().Be("Cafetería Nero");
    }

    [Test]
    public async Task GetByIdAsync_NoExistente_DeberiaDevolverNull()
    {
        // Act
        var guardado = await _repository.GetByIdAsync(99);

        // Assert
        guardado.Should().BeNull();
    }

    [Test]
    public async Task GetAllAsync_DeberiaDevolverTodosOrdenadosPorId()
    {
        // Arrange
        await _repository.CreateAllAsync([CrearUsuario(3, "Ana"), CrearUsuario(1, "Cristian"), CrearUsuario(2, "Jesús")]);

        // Act
        var usuarios = (await _repository.GetAllAsync()).ToList();

        // Assert
        usuarios.Should().HaveCount(3);
        usuarios.Select(u => u.Id).Should().ContainInOrder(1, 2, 3);
    }

    [Test]
    public async Task CreateAllAsync_DeberiaGuardarVariosDeUnaVez()
    {
        // Arrange
        var usuarios = new List<User> { CrearUsuario(1), CrearUsuario(2, "Jesús") };

        // Act
        await _repository.CreateAllAsync(usuarios);

        // Assert
        (await _repository.GetAllAsync()).Should().HaveCount(2);
    }

    [Test]
    public async Task UpdateAsync_Existente_DeberiaGuardarLosCambios()
    {
        // Arrange
        await _repository.CreateAsync(CrearUsuario());
        var modificado = CrearUsuario(name: "Cristian Álvarez");

        // Act
        var resultado = await _repository.UpdateAsync(modificado);

        // Assert
        resultado.Should().NotBeNull();
        (await _repository.GetByIdAsync(1))!.Name.Should().Be("Cristian Álvarez");
    }

    [Test]
    public async Task UpdateAsync_NoExistente_DeberiaDevolverNull()
    {
        // Act
        var resultado = await _repository.UpdateAsync(CrearUsuario(id: 99));

        // Assert
        resultado.Should().BeNull();
    }

    [Test]
    public async Task DeleteAsync_Existente_DeberiaDevolverTrue()
    {
        // Arrange
        await _repository.CreateAsync(CrearUsuario());

        // Act
        var borrado = await _repository.DeleteAsync(1);

        // Assert
        borrado.Should().BeTrue();
        (await _repository.GetAllAsync()).Should().BeEmpty();
    }

    [Test]
    public async Task DeleteAsync_NoExistente_DeberiaDevolverFalse()
    {
        // Act
        var borrado = await _repository.DeleteAsync(99);

        // Assert
        borrado.Should().BeFalse();
    }

    [Test]
    public async Task DeleteAllAsync_DeberiaVaciarLaTabla()
    {
        // Arrange
        await _repository.CreateAllAsync([CrearUsuario(1), CrearUsuario(2, "Jesús")]);

        // Act
        await _repository.DeleteAllAsync();

        // Assert
        (await _repository.GetAllAsync()).Should().BeEmpty();
    }
}