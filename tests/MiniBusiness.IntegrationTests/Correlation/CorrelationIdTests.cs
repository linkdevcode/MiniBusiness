using System.Net;
using FluentAssertions;
using MiniBusiness.IntegrationTests.Infrastructure;

namespace MiniBusiness.IntegrationTests.Correlation;

[Collection("PostgresCollection")]
public class CorrelationIdTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();
    private const string CorrelationHeader = "X-Correlation-ID";

    [Fact]
    public async Task OK_WhenNoCorrelationHeader_ResponseContainsGeneratedId()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.TryGetValues(CorrelationHeader, out var values).Should().BeTrue();
        values!.Single().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task OK_WhenClientSendsCorrelationHeader_ResponseEchoesSameId()
    {
        var correlationId = Guid.NewGuid().ToString();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.TryAddWithoutValidation(CorrelationHeader, correlationId);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues(CorrelationHeader).Single().Should().Be(correlationId);
    }
}
