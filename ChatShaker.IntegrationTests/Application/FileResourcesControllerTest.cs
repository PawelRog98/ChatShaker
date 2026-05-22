using System.Net.Http.Json;
using ChatShaker.Api.Helpers;
using ChatShaker.IntegrationTests.Fixtures;
using FluentAssertions;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace ChatShaker.IntegrationTests.Application;

[Collection("IntegrationTests")]
public class FileResourcesControllerTest : IntegrationTestBase
{
    public FileResourcesControllerTest(IntegrationTestsWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task Upload_ReturnBadRequest_WhenFileIsEmpty()
    {
        await GetTokenForAuth();

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Array.Empty<byte>());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(fileContent, "File", "test.jpg");

        var response = await HttpClient.PostAsync("api/fileresources/upload", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
