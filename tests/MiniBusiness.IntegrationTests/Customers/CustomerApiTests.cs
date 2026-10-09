using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MiniBusiness.Application.Common;
using MiniBusiness.Application.Customers.DTOs;
using MiniBusiness.Domain.Enums;
using MiniBusiness.IntegrationTests.Infrastructure;

namespace MiniBusiness.IntegrationTests.Customers;

[Collection("PostgresCollection")]
public class CustomerApiTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Chỉ reset dữ liệu DB về trạng thái trống sau mỗi test case
        await factory.ResetDatabaseAsync();
    }
    
    [Fact]
    public async Task OK_Create_ThenGetById_ReturnsSameData()
    {
        // Arrange
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var name = "Nguyen Van A";
        var payload = new { name, email };

        // Act — Create
        var createResponse = await _client.PostAsJsonAsync("/api/customers", payload);

        // Assert — Create
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content
            .ReadFromJsonAsync<CustomerDto>(JsonOptions);

        created.Should().NotBeNull();
        created!.Email.Should().Be(email);
        created.Name.Should().Be(name);

        // Act — GetById
        var getResponse = await _client.GetAsync($"/api/customers/{created.Id}");

        // Assert — GetById
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await getResponse.Content
            .ReadFromJsonAsync<CustomerDto>(JsonOptions);

        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(created.Id);
        fetched.Email.Should().Be(email);
        fetched.Name.Should().Be(name);
    }
    
    [Fact]
    public async Task NG_Create_WhenDuplicateEmail_Returns409()
    {
        // Arrange
        var email = $"dup-{Guid.NewGuid():N}@example.com";
        var payload = new { name = "First", email };

        var first = await _client.PostAsJsonAsync("/api/customers", payload);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act — POST lần 2 cùng email
        var second = await _client.PostAsJsonAsync("/api/customers", payload);
        
        // Assert
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    [Fact]
    public async Task OK_GetAll_WhenThreeCustomers_PageSize2_ReturnsPagedResult()
    {
        // Arrange — tạo 3 customer với email unique
        for (int i = 0; i < 3; i++)
        {
            var email = $"user-{Guid.NewGuid():N}@example.com";
            var name = $"Nguyen Van {i}";
            var payload = new { name, email };
            await _client.PostAsJsonAsync("/api/customers", payload);
        }

        // Act
        var getResponse = await _client.GetAsync("/api/customers?page=1&pageSize=2");
        var customersResult = await getResponse.Content.ReadFromJsonAsync<PagedResult<CustomerDto>>(JsonOptions);

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        customersResult?.Items.Count.Should().Be(2);
        customersResult?.Page.Should().Be(1);
        customersResult?.PageSize.Should().Be(2);
        customersResult?.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task OK_GetById_WhenOneCustomer_ReturnsSameData()
    {
        // Arrange
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var name = $"Nguyen Van A";
        var payload = new { name, email };
        var createCustomerResponse = await _client.PostAsJsonAsync("/api/customers", payload);
        var creatingCustomer = await createCustomerResponse.Content
            .ReadFromJsonAsync<CustomerDto>(JsonOptions);
        
        // Act
        var getCustomerResponse = await _client.GetAsync($"/api/customers/{creatingCustomer?.Id}");
        var gettingCustomer = await getCustomerResponse.Content.ReadFromJsonAsync<CustomerDto>(JsonOptions);
        
        // Assert
        createCustomerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        getCustomerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        gettingCustomer?.Id.Should().Be(creatingCustomer!.Id);
        gettingCustomer?.Name.Should().Be(name);
        gettingCustomer?.Email.Should().Be(email);
    }

    [Fact]
    public async Task NG_GetById_WhenMissing_Returns404()
    {
        // Act
        var getCustomerResponse = await _client.GetAsync($"/api/customers/{Guid.NewGuid():N}");
        
        // Assert
        getCustomerResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OK_Update_ThenGetById_ReturnsNewData()
    {
        // Arrange
        var beforeEmail = $"user-{Guid.NewGuid():N}@example.com";
        var beforeName = $"Nguyen Van A";
        var createCustomerResponse = await _client.PostAsJsonAsync("/api/customers", new
        {
            name = beforeName,
            email = beforeEmail,
        });
        var creatingCustomer = await createCustomerResponse.Content
            .ReadFromJsonAsync<CustomerDto>(JsonOptions);
        
        // Act
        var afterEmail = $"user-{Guid.NewGuid():N}@example.com";
        var afterName = $"Nguyen Van B";
        var updateCustomerResponse = await _client.PutAsJsonAsync($"/api/customers/{creatingCustomer?.Id}", new
        {
            name = afterName,
            email = afterEmail,
        });
        var updatingCustomer = await updateCustomerResponse.Content.ReadFromJsonAsync<CustomerDto>(JsonOptions);
        
        // Assert
        createCustomerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        creatingCustomer?.Name.Should().Be(beforeName);
        creatingCustomer?.Email.Should().Be(beforeEmail);
        
        updateCustomerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        updatingCustomer?.Id.Should().Be(creatingCustomer!.Id);
        updatingCustomer?.Name.Should().Be(afterName);
        updatingCustomer?.Email.Should().Be(afterEmail);
    }

    [Fact]
    public async Task NG_Update_WhenMissing_Returns404()
    {
        // Act
        var updateCustomerResponse = await _client.PutAsJsonAsync($"/api/customers/{Guid.NewGuid():N}", new
        {
            name = "Nguyen Van A",
            email = "userA@example.com",
        });
        
        // Assert
        updateCustomerResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OK_Deactivate_Returns204()
    {
        // Arrange
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var name = $"Nguyen Van A";
        var payload = new { name, email };
        var createCustomerResponse = await _client.PostAsJsonAsync("/api/customers", payload);
        var creatingCustomer = await createCustomerResponse.Content
            .ReadFromJsonAsync<CustomerDto>(JsonOptions);
        
        // Act
        var deactivateCustomerResponse = await _client.DeleteAsync($"/api/customers/{creatingCustomer?.Id}");
        var getCustomerResponse = await _client.GetAsync($"/api/customers/{creatingCustomer?.Id}");
        var gettingCustomer = await getCustomerResponse.Content.ReadFromJsonAsync<CustomerDto>(JsonOptions);
        
        // Assert
        createCustomerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        creatingCustomer?.Status.Should().Be(nameof(CustomerStatus.Active));
        
        deactivateCustomerResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        getCustomerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        gettingCustomer?.Status.Should().Be(nameof(CustomerStatus.Inactive));
    }

    [Fact]
    public async Task NG_Deactivate_WhenMissing_Returns404()
    {
        // Act
        var deactivateCustomerResponse = await _client.DeleteAsync($"/api/customers/{Guid.NewGuid():N}");
        
        // Assert
        deactivateCustomerResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}