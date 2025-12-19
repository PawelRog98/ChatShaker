using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Transactions;
using ChatShaker.Api.Helpers;
using ChatShaker.Application.Users.Commands.Login;
using ChatShaker.Application.Users.Commands.Shared;
using ChatShaker.Domain.Entities;
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
        Console.WriteLine("Clear");
        await Context.Database.ExecuteSqlRawAsync("DELETE FROM Tokens");
        await Context.Database.ExecuteSqlRawAsync("DELETE FROM ChatRoomMemberships");
        await Context.Database.ExecuteSqlRawAsync("DELETE FROM ChatRoomKeyBlobs");
        await Context.Database.ExecuteSqlRawAsync("DELETE FROM ChatRooms");
        await Context.Database.ExecuteSqlRawAsync("DELETE FROM Messages");
        await Context.Database.ExecuteSqlRawAsync("DELETE FROM Users");
        await Context.Database.ExecuteSqlRawAsync("DELETE FROM Roles");

        await Context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Roles', RESEED, 0)");
        await Context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Users', RESEED, 0)");
        await Context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Tokens', RESEED, 0)"); 
        
    }

    protected async Task GetTokenForAuth()
    {
        var loginDto = new LoginDto
        {
            Email = "test@test.com",
            Password = "testPassword-1"
        };

        var response = await HttpClient.PostAsJsonAsync<LoginDto>("api/auth/login", loginDto);

        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<Response<AuthTokenDto>>();

        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.Data.AccessToken);
    }

    protected async Task<IEnumerable<User>> GetSeededUsersByNumber(int count) =>
         await Context.Users.Include(x => x.Role)
            .Take(count)
            .ToListAsync();

}
