using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MiniBusiness.Application.Customers.DTOs;
using MiniBusiness.IntegrationTests.Infrastructure;

namespace MiniBusiness.IntegrationTests.Customers;

public class CustomerApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient  _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CustomerApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task OK_Create_ThenGetById_ReturnsSameData()
    {
        // Arrange
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var payload = new { name = "Nguyen Van A", email };

        // Act — Create
        var createResponse = await _client.PostAsJsonAsync("/api/customers", payload);

        // Assert — Create
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content
            .ReadFromJsonAsync<CustomerDto>(JsonOptions);

        created.Should().NotBeNull();
        created!.Email.Should().Be(email);
        created.Name.Should().Be("Nguyen Van A");

        // Act — GetById
        var getResponse = await _client.GetAsync($"/api/customers/{created.Id}");

        // Assert — GetById
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await getResponse.Content
            .ReadFromJsonAsync<CustomerDto>(JsonOptions);

        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(created.Id);
        fetched.Email.Should().Be(email);
        fetched.Name.Should().Be("Nguyen Van A");
    }
}