using ChatShaker.Infrastructure.Data;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;

namespace ChatShaker.IntegrationTests.Fixtures;

public class IntegrationTestBase : IClassFixture<IntegrationTestsWebAppFactory>, IDisposable
{

    protected readonly IntegrationTestsWebAppFactory Factory;
    protected readonly IServiceScope Scope;
    protected readonly AppDbContext Context;
    protected readonly IMediator Mediator;
    protected readonly HttpClient HttpClient;

    public IntegrationTestBase(IntegrationTestsWebAppFactory integrationTestsWebAppFactory)
    {
        Factory = integrationTestsWebAppFactory;
        Scope = Factory.Services.CreateScope();
        Context = Factory.Services.GetRequiredService<AppDbContext>();
        Mediator = Factory.Services.GetRequiredService<IMediator>();
        HttpClient = Factory.CreateClient();
    }
    public void Dispose()
    {
        Scope.Dispose();
    }
}
