using System.Threading.Tasks;
using System.Transactions;
using ChatShaker.Infrastructure.Data;
using ChatShaker.IntegrationTests.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;

namespace ChatShaker.IntegrationTests.Fixtures;

public class IntegrationTestBase : IClassFixture<IntegrationTestsWebAppFactory>, IAsyncLifetime
{

    protected IntegrationTestsWebAppFactory Factory;
    protected IServiceScope Scope;
    protected AppDbContext Context;
    protected IMediator Mediator;
    protected HttpClient HttpClient;

    public IntegrationTestBase(IntegrationTestsWebAppFactory integrationTestsWebAppFactory)
    {
        Factory = integrationTestsWebAppFactory;
    }

    public async Task InitializeAsync()
    {
        Scope = Factory.Services.CreateScope();
        Context = Scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        HttpClient = Factory.CreateClient();


        await ClearDatabase();
        await TestDataSeeder.SeedUser(Context);
    }
    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();

        Scope.Dispose();
        HttpClient.Dispose();
    }

    public async Task ClearDatabase()
    {
        await Context.Database.ExecuteSqlRawAsync("DELETE FROM Tokens");
        await Context.Database.ExecuteSqlRawAsync("DELETE FROM Users");
        await Context.Database.ExecuteSqlRawAsync("DELETE FROM Roles");
    }

}
