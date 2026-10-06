using FluentAssertions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using RepositorioRemoto.Cache;
using RepositorioRemoto.Config;
using RepositorioRemoto.Models;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace RepositorioRemoto.Tests.Cache;

/// <summary>
/// Tests de integración de RedisCacheService con un Redis real en TestContainers.
/// </summary>
[TestFixture]
public class RedisCacheServiceTests
{
    private RedisContainer _container = null!;
    private ConnectionMultiplexer _redis = null!;
    private RedisCacheService _cache = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Un solo contenedor para todos los tests de la clase
        _container = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();

        await _container.StartAsync();

        // allowAdmin=true: ClearAsync usa FlushDatabase, que Redis solo permite en modo admin
        _redis = await ConnectionMultiplexer.ConnectAsync(
            $"{_container.GetConnectionString()},allowAdmin=true");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _redis.DisposeAsync();
        await _container.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        // Redis limpio para cada test
        await _redis.GetServer(_redis.GetEndPoints()[0]).FlushDatabaseAsync();

        var config = Options.Create(new CacheConfig { ExpirationSeconds = 60 });
        _cache = new RedisCacheService(_redis, config);
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
    public class CasosPositivos : RedisCacheServiceTests
    {
        [Test]
        public async Task SetAsync_DeberiaGuardarElUsuarioYPoderLeerlo()
        {
            // Arrange
            var usuario = CrearUsuario(1);
            await _cache.SetAsync("user:1", usuario);

            // Act
            var resultado = await _cache.GetAsync<User>("user:1");

            // Assert
            resultado.Should().BeEquivalentTo(usuario);
        }
        
        [Test]
        public async Task RemoveAsync_ClaveExistente_DeberiaBorrarlaYDevolverTrue()
        {
            // Arrange
            await _cache.SetAsync("user:1", CrearUsuario(1));

            // Act
            var borrado = await _cache.RemoveAsync("user:1");

            // Assert
            borrado.Should().BeTrue();
            var resultado = await _cache.GetAsync<User>("user:1");
            resultado.Should().BeNull();
        }

        [Test]
        public async Task ClearAsync_DeberiaVaciarTodaLaCache()
        {
            // Arrange
            await _cache.SetAsync("user:1", CrearUsuario(1));
            await _cache.SetAsync("user:2", CrearUsuario(2));

            // Act
            await _cache.ClearAsync();

            // Assert
            (await _cache.GetAsync<User>("user:1")).Should().BeNull();
            (await _cache.GetAsync<User>("user:2")).Should().BeNull();
        }
        [Test]
        public async Task SetAsync_SinExpiracion_DeberiaUsarLaDeLaConfiguracion()
        {
            // Arrange
            await _cache.SetAsync("user:1", CrearUsuario(1));

            // Act
            var ttl = await _redis.GetDatabase().KeyTimeToLiveAsync("user:1");

            // Assert
            ttl.Should().NotBeNull();
            ttl!.Value.Should().BeGreaterThan(TimeSpan.Zero);
            ttl.Value.Should().BeLessThanOrEqualTo(TimeSpan.FromSeconds(60));
        }
    }

    [TestFixture]
    public class CasosNegativos : RedisCacheServiceTests
    {
        [Test]
        public async Task GetAsync_ClaveInexistente_DeberiaDevolverNull()
        {
            // Act
            var resultado = await _cache.GetAsync<User>("user:999");

            // Assert
            resultado.Should().BeNull();
        }

        [Test]
        public async Task RemoveAsync_ClaveInexistente_DeberiaDevolverFalse()
        {
            // Act
            var borrado = await _cache.RemoveAsync("user:999");

            // Assert
            borrado.Should().BeFalse();
        }
    }
}
