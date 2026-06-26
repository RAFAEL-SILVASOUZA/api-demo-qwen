using Microsoft.AspNetCore.Mvc.Testing;

namespace ProdutosApi.Tests;

public class TestApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync().AsTask();
    }
}
