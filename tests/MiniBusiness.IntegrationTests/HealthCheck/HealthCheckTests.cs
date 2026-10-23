using System.Net;
using FluentAssertions;
using MiniBusiness.IntegrationTests.Infrastructure;

namespace MiniBusiness.IntegrationTests.HealthCheck;

[Collection("PostgresCollection")]
public class HealthCheckTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Chỉ reset dữ liệu DB về trạng thái trống sau mỗi test case
        await factory.ResetDatabaseAsync();
    }
    
    [Fact]
    public async Task Get_Health_Liveness_Returns_Healthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    [Fact]
    public async Task Get_Health_Readiness_Returns_Healthy()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }
}