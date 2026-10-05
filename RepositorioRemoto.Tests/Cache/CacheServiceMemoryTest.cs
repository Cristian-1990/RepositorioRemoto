using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using RepositorioRemoto.Cache;

namespace RepositorioRemoto.Tests.Cache;

[TestFixture]
[TestOf(typeof(CacheService))]
public class CacheServiceTest
{
    
    private MemoryCache _memoryCache = null!;
    private CacheService _cacheService = null!;

    

    [SetUp]
    public void Setup()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cacheService = new CacheService(_memoryCache);
    }
    
    [TearDown]
    public void TearDown()
    {
        _memoryCache.Dispose();
    }


    [Test]
    public async Task SetAsync_TestAndGetAsync()
    {
        var value = "Hola";
        await _cacheService.SetAsync("Clave1", value);
        
        var result  = await _cacheService.GetAsync<string>("Clave1");
        result.Should().Be("Hola");
        
    }

    [Test]
    public async Task GetAsync_Test_ClaveNoExiste()
    {
        var result = await _cacheService.GetAsync<string>("no-existe");
        result.Should().BeNull();
    }

    [Test]
    public async Task SetAsync_Test_Expiracion()
    {
        var value = "Hola";
        await _cacheService.SetAsync("Clave1", value,TimeSpan.FromMilliseconds(100));
        
        await Task.Delay(200);
        
        var result = await _cacheService.GetAsync<string>("Clave1");
        result.Should().BeNull();
    }

    [Test]
    public async Task RemoveAsync_Test_EliminarElemento()
    {
        await _cacheService.SetAsync("Clave1", "Hola");
        var eliminar = await _cacheService.RemoveAsync("Clave1");
        
        var result = await  _cacheService.GetAsync<string>("Clave1");

        eliminar.Should().BeTrue();
        result.Should().BeNull();
    }

    [Test]
    public async Task ClearAsync_Test_TodoEliminar()
    {
        await _cacheService.SetAsync("Clave1", "Hola");
        await _cacheService.SetAsync("Clave2", "Adios");
        
        await _cacheService.ClearAsync();
        
        var result1 = await  _cacheService.GetAsync<string>("Clave1");
        var result2 = await  _cacheService.GetAsync<string>("Clave2"); 
        
        result1.Should().BeNull();
        result2.Should().BeNull();
    }

    [Test]
    public async Task SetAsync_Test_SobreEscribir()
    {
        await _cacheService.SetAsync("Clave1", "Hola");
        await _cacheService.SetAsync("Clave1", "Adios");
        
        var result = await _cacheService.GetAsync<string>("Clave1");
        
        result.Should().Be("Adios");
    }

    
}