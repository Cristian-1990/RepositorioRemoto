using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using RepositorioRemoto.Cache;
using RepositorioRemoto.Config;

namespace RepositorioRemoto.Tests.Cache;

/// <summary>
/// Tests unitarios de MemoryCacheService sobre una caché en memoria real.
/// </summary>
[TestFixture]
public class MemoryCacheServiceTests
{
    private MemoryCache _memoryCache = null!;
    private MemoryCacheService _cacheService = null!;

    [SetUp]
    public void SetUp()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        var config = Options.Create(new CacheConfig { ExpirationSeconds = 60 });
        _cacheService = new MemoryCacheService(_memoryCache, config);
    }

    [TearDown]
    public void TearDown()
    {
        _memoryCache.Dispose();
    }

    [TestFixture]
    public class CasosPositivos : MemoryCacheServiceTests
    {
        [Test]
        public async Task SetAsync_DeberiaGuardarElValor()
        {
            // Arrange
            await _cacheService.SetAsync("user:1", "Cristian");

            // Act
            var resultado = await _cacheService.GetAsync<string>("user:1");

            // Assert
            resultado.Should().Be("Cristian");
        }

        [Test]
        public async Task RemoveAsync_ClaveExistente_DeberiaDevolverTrue()
        {
            // Arrange
            await _cacheService.SetAsync("user:1", "Cristian");

            // Act
            var borrado = await _cacheService.RemoveAsync("user:1");

            // Assert
            borrado.Should().BeTrue();
            var resultado = await _cacheService.GetAsync<string>("user:1");
            resultado.Should().BeNull();
        }

        [Test]
        public async Task ClearAsync_DeberiaVaciarLaCacheEntera()
        {
            // Arrange
            await _cacheService.SetAsync("user:1", "Cristian");
            await _cacheService.SetAsync("user:2", "Jesus");

            // Act
            await _cacheService.ClearAsync();

            // Assert
            var primero = await _cacheService.GetAsync<string>("user:1");
            var segundo = await _cacheService.GetAsync<string>("user:2");
            primero.Should().BeNull();
            segundo.Should().BeNull();
        }
    }

    [TestFixture]
    public class CasosNegativos : MemoryCacheServiceTests
    {
        [Test]
        public async Task GetAsync_ClaveInexistente_DeberiaDevolverNull()
        {
            // Act
            var resultado = await _cacheService.GetAsync<string>("user:999");

            // Assert
            resultado.Should().BeNull();
        }

        [Test]
        public async Task RemoveAsync_ClaveInexistente_DeberiaDevolverFalse()
        {
            // Act
            var borrado = await _cacheService.RemoveAsync("user:999");

            // Assert
            borrado.Should().BeFalse();
        }
    }
}
