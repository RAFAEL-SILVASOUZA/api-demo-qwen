namespace ProdutosApi.Tests;

[CollectionDefinition(nameof(ServiceTests))]
public class ServiceTestsCollection : ICollectionFixture<ServiceTestDatabase>
{
}

[Collection(ServiceTests)]
public class ServiceTestDatabase : IAsyncLifetime
{
    private readonly AppDbContext _context;

    public ServiceTestDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=produtosapi_tests;Username=postgres;Password=postgres")
            .Options;

        _context = new AppDbContext(options);
    }

    public Task InitializeAsync()
    {
        return _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync().AsTask();
    }

    public AppDbContext Context => _context;
}
