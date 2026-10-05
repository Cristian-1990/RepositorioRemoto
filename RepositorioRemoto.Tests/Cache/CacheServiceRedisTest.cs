using FluentAssertions;
using NUnit.Framework.Internal;
using RepositorioRemoto.Cache;
using RepositorioRemoto.Models;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace RepositorioRemoto.Tests.Cache;

[TestFixture]
[TestOf(typeof(CacheServiceRedis))]
public class CacheServiceRedisTest
{
    private RedisContainer _redisContainer = null!;
    private IConnectionMultiplexer _connection = null!;
    private CacheServiceRedis _cache = null!;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _redisContainer = new RedisBuilder().Build();
        await _redisContainer.StartAsync();
        
        _connection = await ConnectionMultiplexer.ConnectAsync(_redisContainer.GetConnectionString());
        
        _cache = new CacheServiceRedis(_connection);
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await _redisContainer.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Test]
    public async Task SetAsync_GetAsync_GuardarYRecuperar()
    {
        var usuario = new User(
            1,
            "Jesus",
            "JCobo",
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
                "InforDev",
                "Desarrollo de software",
                "web development"
            ));
        await _cache.SetAsync("user:1", usuario);
        var result = await  _cache.GetAsync<User>("user:1");

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(usuario);
    }

    [Test]
    public async Task GetAsync_ClaveNoExiste()
    {
        var result = await _cache.GetAsync<User>("user:1");
        result.Should().BeNull();
    }
    
    [Test] 
        public async Task RemoveAsync_ClaveExistente()
        {
        var usuario = new User(
                    1,
                    "Jesus",
                    "JCobo",
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
                        "InforDev",
                        "Desarrollo de software",
                        "web development"
                    ));
        await _cache.SetAsync("user:1", usuario);
        var eliminar = await _cache.RemoveAsync("user:1");
        var result = await _cache.GetAsync<User>("user:1");

        eliminar.Should().BeTrue();
        result.Should().BeNull();
        }
        
    [Test]
        public async Task RemoveAsync_ClaveNoExiste(){
        var result = await  _cache.RemoveAsync("user:999");
        result.Should().BeFalse();
    }
        
    [Test]
        public async Task ClaarAsync_EliminarTodo(){
        var usuario1 = new User(
                            1,
                            "Jesus",
                            "JCobo",
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
                                "InforDev",
                                "Desarrollo de software",
                                "web development"
                            ));
        
        var usuario2 = new User(
            2,
            "Cristian",
            "Cobo",
            "Cristian@email.com",
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
                "InforDev",
                "Desarrollo de software",
                "web development"
            ));
        await _cache.SetAsync("user:1",usuario1);
        await _cache.SetAsync("user:2", usuario2);

        await _cache.ClearAsync();
        
        var result1 = await _cache.GetAsync<User>("user:1");
        var result2 = await _cache.GetAsync<User>("user:2");

        result1.Should().BeNull();
        result2.Should().BeNull();
        }
        
        [Test]
            public async Task SetAsync_Expiracion(){
                var usuario = new User(
                    1,
                    "Jesus",
                    "JCobo",
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
                        "InforDev",
                        "Desarrollo de software",
                        "web development"
                    ));
                await _cache.SetAsync("user:1", usuario, TimeSpan.FromMilliseconds(500));
                await Task.Delay(1000);
                var result = await _cache.GetAsync<User>("user:1");
                result.Should().BeNull();

            }
    
   
}