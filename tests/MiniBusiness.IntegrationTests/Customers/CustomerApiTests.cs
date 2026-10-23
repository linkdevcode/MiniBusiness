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
        var createResponse = await _client.PostAsJsonAsync("/api/v1/customers", payload);

        // Assert — Create
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content
            .ReadFromJsonAsync<CustomerDto>(JsonOptions);

        created.Should().NotBeNull();
        created!.Email.Should().Be(email);
        created.Name.Should().Be(name);
        created.CreatedAt.Should().NotBe(default);
        created.UpdatedAt.Should().NotBe(default);
        (created.UpdatedAt - created.CreatedAt).Duration().Should().BeLessThan(TimeSpan.FromSeconds(1));

        // Act — GetById
        var getResponse = await _client.GetAsync($"/api/v1/customers/{created.Id}");

        // Assert — GetById
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await getResponse.Content
            .ReadFromJsonAsync<CustomerDto>(JsonOptions);

        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(created.Id);
        fetched.CreatedAt.Should().Be(created.CreatedAt);
        fetched.UpdatedAt.Should().Be(created.UpdatedAt);
        fetched.Email.Should().Be(email);
        fetched.Name.Should().Be(name);
    }
    
    [Fact]
    public async Task NG_Create_WhenDuplicateEmail_Returns409()
    {
        // Arrange
        var email = $"dup-{Guid.NewGuid():N}@example.com";
        var payload = new { name = "First", email };

        var first = await _client.PostAsJsonAsync("/api/v1/customers", payload);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act — POST lần 2 cùng email
        var second = await _client.PostAsJsonAsync("/api/v1/customers", payload);
        
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
            await _client.PostAsJsonAsync("/api/v1/customers", payload);
        }

        // Act
        var getResponse = await _client.GetAsync("/api/v1/customers?page=1&pageSize=2");
        var customersResult = await getResponse.Content.ReadFromJsonAsync<PagedResult<CustomerDto>>(JsonOptions);

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        customersResult?.Items.Count.Should().Be(2);
        customersResult?.Page.Should().Be(1);
        customersResult?.PageSize.Should().Be(2);
        customersResult?.TotalCount.Should().Be(3);
        customersResult?.TotalPages.Should().Be(2);
    }
    
    [Fact]
    public async Task NG_GetAll_WithLargePageSize()
    {
        // Act
        var getResponse = await _client.GetAsync("/api/v1/customers?page=1&pageSize=9999");
        
        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task OK_GetById_WhenOneCustomer_ReturnsSameData()
    {
        // Arrange
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var name = $"Nguyen Van A";
        var payload = new { name, email };
        var createCustomerResponse = await _client.PostAsJsonAsync("/api/v1/customers", payload);
        var creatingCustomer = await createCustomerResponse.Content
            .ReadFromJsonAsync<CustomerDto>(JsonOptions);
        
        // Act
        var getCustomerResponse = await _client.GetAsync($"/api/v1/customers/{creatingCustomer?.Id}");
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
        var getCustomerResponse = await _client.GetAsync($"/api/v1/customers/{Guid.NewGuid():N}");
        
        // Assert
        getCustomerResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OK_Update_ThenGetById_ReturnsNewData()
    {
        // Arrange
        var beforeEmail = $"user-{Guid.NewGuid():N}@example.com";
        var beforeName = "Nguyen Van A";
        
        var createResponse = await _client.PostAsJsonAsync("/api/v1/customers", new
        {
            name = beforeName,
            email = beforeEmail,
        });
        
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>(JsonOptions);
        createdCustomer.Should().NotBeNull();
        
        // Assert Create Timestamps
        createdCustomer!.CreatedAt.Should().NotBe(default);
        createdCustomer.UpdatedAt.Should().NotBe(default);
        (createdCustomer.UpdatedAt - createdCustomer.CreatedAt).Duration().Should().BeLessThan(TimeSpan.FromSeconds(1));
        
        // Act - Update
        var afterEmail = $"user-{Guid.NewGuid():N}@example.com";
        var afterName = "Nguyen Van B";
        
        // Thêm delay nhỏ để đảm bảo thời gian hệ thống kịp nhảy mốc mới (nếu chạy quá nhanh)
        await Task.Delay(10);

        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/customers/{createdCustomer.Id}", new
        {
            name = afterName,
            email = afterEmail,
        });
        
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedCustomer = await updateResponse.Content.ReadFromJsonAsync<CustomerDto>(JsonOptions);
        updatedCustomer.Should().NotBeNull();
        
        // Assert Update Response
        updatedCustomer!.Id.Should().Be(createdCustomer.Id);
        updatedCustomer.Name.Should().Be(afterName);
        updatedCustomer.Email.Should().Be(afterEmail);
        updatedCustomer.CreatedAt.Should().Be(createdCustomer.CreatedAt); // CreatedAt phải giữ nguyên
        updatedCustomer.UpdatedAt.Should().BeAfter(createdCustomer.UpdatedAt); // UpdatedAt phải mới hơn

        // Act & Assert - GetById (Đúng theo tên test case)
        var getResponse = await _client.GetAsync($"/api/v1/customers/{createdCustomer.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var fetchedCustomer = await getResponse.Content.ReadFromJsonAsync<CustomerDto>(JsonOptions);
        fetchedCustomer.Should().NotBeNull();
        
        fetchedCustomer!.Id.Should().Be(createdCustomer.Id);
        fetchedCustomer.Name.Should().Be(afterName);
        fetchedCustomer.Email.Should().Be(afterEmail);
        fetchedCustomer.CreatedAt.Should().Be(createdCustomer.CreatedAt);
        fetchedCustomer.UpdatedAt.Should().Be(updatedCustomer.UpdatedAt);
    }

    [Fact]
    public async Task NG_Update_WhenMissing_Returns404()
    {
        // Act
        var updateCustomerResponse = await _client.PutAsJsonAsync($"/api/v1/customers/{Guid.NewGuid():N}", new
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
        var createCustomerResponse = await _client.PostAsJsonAsync("/api/v1/customers", payload);
        var creatingCustomer = await createCustomerResponse.Content
            .ReadFromJsonAsync<CustomerDto>(JsonOptions);
        
        // Act
        var deactivateCustomerResponse = await _client.DeleteAsync($"/api/v1/customers/{creatingCustomer?.Id}");
        var getCustomerResponse = await _client.GetAsync($"/api/v1/customers/{creatingCustomer?.Id}");
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
        var deactivateCustomerResponse = await _client.DeleteAsync($"/api/v1/customers/{Guid.NewGuid():N}");
        
        // Assert
        deactivateCustomerResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task NG_Deactivate_WhenAlreadyInactive_Returns400()
    {
        // Arrange
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var name = $"Nguyen Van A";
        var payload = new { name, email };
        
        var createCustomerResponse = await _client.PostAsJsonAsync("/api/v1/customers", payload);
        var creatingCustomer = await createCustomerResponse.Content.ReadFromJsonAsync<CustomerDto>(JsonOptions);
        var deactivateCustomerResponseFirst = await _client.DeleteAsync($"/api/v1/customers/{creatingCustomer?.Id}");
        
        // Act
        var deactivateCustomerResponse = await _client.DeleteAsync($"/api/v1/customers/{creatingCustomer?.Id}");
        
        // Assert
        createCustomerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        creatingCustomer?.Status.Should().Be(nameof(CustomerStatus.Active));
        deactivateCustomerResponseFirst.StatusCode.Should().Be(HttpStatusCode.NoContent);
        deactivateCustomerResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task OK_Reactivate_Returns204()
    {
        // Arrange
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var name = $"Nguyen Van A";
        var payload = new { name, email };
        
        var createCustomerResponse = await _client.PostAsJsonAsync("/api/v1/customers", payload);
        var creatingCustomer = await createCustomerResponse.Content.ReadFromJsonAsync<CustomerDto>(JsonOptions);
        var deactivateCustomerResponse = await _client.DeleteAsync($"/api/v1/customers/{creatingCustomer?.Id}");
        
        // Act
        var reactivateCustomerResponse = await _client.PostAsJsonAsync($"/api/v1/customers/{creatingCustomer?.Id}/reactivate", new{});
        var getCustomerResponse = await _client.GetAsync($"/api/v1/customers/{creatingCustomer?.Id}");
        var gettingCustomer = await getCustomerResponse.Content.ReadFromJsonAsync<CustomerDto>(JsonOptions);
        
        // Assert
        createCustomerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        deactivateCustomerResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        reactivateCustomerResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        getCustomerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        gettingCustomer?.Status.Should().Be(nameof(CustomerStatus.Active));
    }

    [Fact]
    public async Task NG_Reactivate_WhenMissing_Returns404()
    {
        // Act
        var reactivateCustomerResponse = await _client.PostAsJsonAsync($"/api/v1/customers/{Guid.NewGuid():N}/reactivate", new{});
        
        // Assert
        reactivateCustomerResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task NG_Reactivate_WhenAlreadyActive_Returns400()
    {
        // Arrange
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var name = $"Nguyen Van A";
        var payload = new { name, email };
        
        var createCustomerResponse = await _client.PostAsJsonAsync("/api/v1/customers", payload);
        var creatingCustomer = await createCustomerResponse.Content.ReadFromJsonAsync<CustomerDto>(JsonOptions);
        
        // Act
        var reactivateCustomerResponse = await _client.PostAsJsonAsync($"/api/v1/customers/{creatingCustomer?.Id}/reactivate", new{});
        
        // Assert
        createCustomerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        creatingCustomer?.Status.Should().Be(nameof(CustomerStatus.Active));
        reactivateCustomerResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task NG_Create_WhenParallelDuplicateEmail_OneCreatedOneConflict()
    {
        // Arrange
        var email = $"race-{Guid.NewGuid():N}@example.com";
        var payload = new { name = "Race User", email };

        // Act
        var task1 = _client.PostAsJsonAsync("/api/v1/customers", payload);
        var task2 = _client.PostAsJsonAsync("/api/v1/customers", payload);
        var responses = await Task.WhenAll(task1, task2);

        var statuses = responses
            .Select(r => r.StatusCode)
            .OrderBy(s => s)
            .ToArray();

        // Assert
        statuses.Should().Equal(
            HttpStatusCode.Created,      // 201
            HttpStatusCode.Conflict);    // 409

        responses.Should().NotContain(r => r.StatusCode == HttpStatusCode.InternalServerError);
    }
}