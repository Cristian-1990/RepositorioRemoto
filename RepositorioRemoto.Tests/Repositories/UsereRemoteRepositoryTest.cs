using System.Net;
using FluentAssertions;
using Moq;
using Refit;
using RepositorioRemoto.Api;
using RepositorioRemoto.Dto;
using RepositorioRemoto.Errors;
using RepositorioRemoto.Repositories;

namespace RepositorioRemoto.Tests.Repositories;

[TestFixture]
[TestOf(typeof(UsereRemoteRepository))]
public class UsereRemoteRepositoryTest
{
    private Mock<IJsonPlaceholderApi> _api = null!;
    private UsereRemoteRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _api = new Mock<IJsonPlaceholderApi>();
        _repository = new UsereRemoteRepository(_api.Object);
    }

    [Test]
    public async Task GetAllAsync()
    {
        var usuariosDto = new List<JsonPlaceholderUserDto>
        {
            CrearUsuarioDto(1),
            CrearUsuarioDto(2)
        };
        
        _api.Setup(x => x.GetAllAsync()).ReturnsAsync(usuariosDto);
        
        var result = await _repository.GetAllAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Test]
    public async Task GetByIdAsync()
    {
        var usuarioDto = CrearUsuarioDto(1);
        _api.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(usuarioDto);
        var result = await _repository.GetByIdAsync(1);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1);
        result.Value.Name.Should().Be("Jesus");
        result.Value.Username.Should().Be("Jcobo");
        result.Value.Email.Should().Be("jesus@email.com");

    }

    [Test]
    public async Task GetByIdAsync_UsuarioNoExiste()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://jsonplaceholder.typicode.com/users/999");
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        var exception = await ApiException.Create(request, HttpMethod.Get, response, new RefitSettings());
        _api.Setup(x => x.GetByIdAsync(999)).ThrowsAsync(exception);
        
        var result = await _repository.GetByIdAsync(999);
        
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<DomainError.NotFound>();
    }

    [Test]
    public async Task CreateAsync()
    {
        var request = new CreateUserRequest(
                "Jesus",
                "Jcobo",
                "jesus@gmail.com",
                new AddressDto(
                    "Calle Mayor",
                    "1A",
                    "Madrid",
                    "28001",
                    new GeoDto(
                        "40.4168",
                        "-3.7038"
                    )
                ),
                "600123123",
                "jesusweb.com",
                new CompanyDto(
                    "jesusDev",
                    "Software",
                    "web development"
                )
            );    
            
        var usuarioDto = new JsonPlaceholderUserDto(
            11,
            "Jesus",
            "Jcobo",
            "jesus@gmail.com",
            request.Address,
            request.Email,
            request.Website,
            request.Company
        );
        _api.Setup(x => x.CreateAsync(It.IsAny<JsonPlaceholderUserDto>())).ReturnsAsync(usuarioDto);
        var result = await _repository.CreateAsync(request);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(11);
        result.Value.Name.Should().Be("Jesus");
        result.Value.Username.Should().Be("Jcobo");
        result.Value.Email.Should().Be("jesus@gmail.com");
    }

    [Test]
    public async Task UpdateAsync()
    {
        var request = new UpdateUserRequest(
            1,
            "Jesus Actualizado",
            "Jcobo",
            "jesus@gmail.com",
            new AddressDto(
                "Calle Mayor",
                "1A",
                "Madrid",
                "28001",
                new GeoDto(
                    "40.4168",
                    "-3.7038"
                )
            ),
            "600123123",
            "jesusweb.com",
            new CompanyDto(
                "jesusDev",
                "Software",
                "web development"
            ));

        var usuarioDto = new JsonPlaceholderUserDto(
            1,
            "Jesus Actualizado",
            "Jcobo",
            "jesus@gmail.com",
            request.Address,
            request.Phone,
            request.Website,
            request.Company
        );
        _api.Setup(x => x.UpdateAsync(1, It.IsAny<JsonPlaceholderUserDto>())).ReturnsAsync(usuarioDto);
        var result = await _repository.UpdateAsync(1, request);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1);
        result.Value.Name.Should().Be("Jesus Actualizado");
        result.Value.Username.Should().Be("Jcobo");
        result.Value.Email.Should().Be("jesus@gmail.com");
        
        
    }

    [Test]
    public async Task DeleteAsync()
    {
        _api.Setup(x => x.DeleteAsync(1)).Returns((Task.CompletedTask));
        var result = await _repository.DeleteAsync(1);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Test]
    public async Task CreateAsync_fallo()
    {
        var request = new CreateUserRequest(
                "Jesus",
                "Jcobo",
                "jesus@gmail.com",
                new AddressDto(
                    "Calle Mayor",
                    "1A",
                    "Madrid",
                    "28001",
                    new GeoDto("40.4168", "-3.7038")
                ),
                "600123123",
                "jesusweb.com",
                new CompanyDto(
                    "jesusDev",
                    "Software",
                    "web development"
                )
            );
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://jsonplaceholder.typicode.com/users");
        
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        
        var exception = await ApiException.Create(httpRequest, HttpMethod.Post, response, new RefitSettings());
        
        _api.Setup(x => x.CreateAsync(It.IsAny<JsonPlaceholderUserDto>())).ThrowsAsync(exception);
        var result = await _repository.CreateAsync(request);
        
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<DomainError.ApiError>();
        

    }

    [Test]
    public async Task UpdateAsync_fallo()
    {
        var request = new UpdateUserRequest(
            999,
            "Jesus",
            "Jcobo",
            "jesus@gmail.com",
            new AddressDto(
                "Calle Mayor",
                "1A",
                "Madrid",
                "28001",
                new GeoDto("40.4168", "-3.7038")
            ),
            "600123123",
            "jesusweb.com",
            new CompanyDto(
                "jesusDev",
                "Software",
                "web development"
            )
        );
        
        

        var httpRequest = new HttpRequestMessage(HttpMethod.Put, "http://jsonplaceholder.typicode.com/users/999");
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var exception = await ApiException.Create(httpRequest, HttpMethod.Put, response, new RefitSettings());
        
        _api.Setup(x => x.UpdateAsync(999, It.IsAny<JsonPlaceholderUserDto>())).ThrowsAsync(exception);

        var result = await _repository.UpdateAsync(999, request);
        
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<DomainError.NotFound>();

    }

    [Test]
    public async Task DeleteAsync_NoUsuario()
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "http://jsonplaceholder.typicode.com/users/999");
        
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        
        var exception = await ApiException.Create(httpRequest, HttpMethod.Delete, response, new RefitSettings());
        _api.Setup(x => x.DeleteAsync(999)).ThrowsAsync(exception);
        
        var result = await _repository.DeleteAsync(999);
        
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<DomainError.NotFound>();
    }


    private static JsonPlaceholderUserDto CrearUsuarioDto(int id)
    {
        return new JsonPlaceholderUserDto(
            id,
            "Jesus",
            "Jcobo",
            "jesus@email.com",
            new AddressDto(
                "Calle Mayor",
                "1A",
                "Madrid",
                "28001",
                new GeoDto(
                    "40.4168",
                    "-3.7038"
                )
            ),
            "600123123",
            "jesusweb.com",
            new CompanyDto(
                "jesusDev",
                "Software",
                "web development"
            )
        );
    }
}