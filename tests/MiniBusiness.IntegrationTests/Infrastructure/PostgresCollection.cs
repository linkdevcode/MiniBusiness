namespace MiniBusiness.IntegrationTests.Infrastructure;

[CollectionDefinition("PostgresCollection")]
public class PostgresCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}