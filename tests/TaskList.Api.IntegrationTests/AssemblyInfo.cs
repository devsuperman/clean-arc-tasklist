using TaskList.Api.IntegrationTests.Infrastructure;

// One PostgreSQL container for the whole assembly. Tests run one at a time (xunit.runner.json)
// because they share the database.
[assembly: AssemblyFixture(typeof(ApiFactory))]
