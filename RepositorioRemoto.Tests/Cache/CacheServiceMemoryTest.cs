using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using RepositorioRemoto.Cache;
using RepositorioRemoto.Models;

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
        await _cacheService.SetAsync("user1", value);
        
        var result  = await _cacheService.GetAsync<string>("user1");
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
        var usuario = CrearUsuario(1);
        await _cacheService.SetAsync("user1", usuario,TimeSpan.FromMilliseconds(100));
        
        await Task.Delay(200);
        
        var result = await _cacheService.GetAsync<string>("user1");
        result.Should().BeNull();
    }

    [Test]
    public async Task RemoveAsync_Test_EliminarElemento()
    {
        await _cacheService.SetAsync("user1", CrearUsuario(1));
        var eliminar = await _cacheService.RemoveAsync("user1");
        
        var result = await  _cacheService.GetAsync<string>("user1");

        eliminar.Should().BeTrue();
        result.Should().BeNull();
    }

    [Test]
    public async Task ClearAsync_Test_TodoEliminar()
    {
        await _cacheService.SetAsync("user1", CrearUsuario(1));
        await _cacheService.SetAsync("user2", CrearUsuario(1));
        
        await _cacheService.ClearAsync();
        
        var result1 = await  _cacheService.GetAsync<string>("user1");
        var result2 = await  _cacheService.GetAsync<string>("user2"); 
        
        result1.Should().BeNull();
        result2.Should().BeNull();
    }

    [Test]
    public async Task SetAsync_Test_SobreEscribir()
    {
        var usuarioOriginal = CrearUsuario(1);
        
        var usuarioActualizado = usuarioOriginal with
        {
            Name = "JesusActualizado"
        };
        
        await _cacheService.SetAsync("user1", usuarioOriginal);
        await _cacheService.SetAsync("user1", usuarioActualizado);
        
        var result = await _cacheService.GetAsync<User>("user1");
        
        result.Should().BeEquivalentTo(usuarioActualizado);
    }
    
    private static User CrearUsuario(int id)
    {
        return new User(
            id,
            "Jesus",
            "Jcobo",
            "jesus@email.com",
            new Address(
                "Calle Mayor",
                "1A",
                "Madrid",
                "28001",
                new Geo(
                    "40.4168",
                    "-3.7038"
                )
            ),
            "600123123",
            "jesusweb.com",
            new Company(
                "jesusDev",
                "Software",
                "web development"
            )
        );
    }

    
}