using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Services.Interfaces;

namespace ChatShaker.ChatMauiApp.Services.Api;

public class FileApiService : IFileApiService
{
    private readonly HttpClient _httpClient;

    public FileApiService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("ShakerApiClient");
    }

    public async Task<Response<string>> UploadFile(byte[] fileBytes, string fileName, string contentType, Guid roomId)
    {
        using var content = new MultipartFormDataContent();
        
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        
        content.Add(fileContent, "File", fileName);
        content.Add(new StringContent(roomId.ToString()), "RoomId");

        var response = await _httpClient.PostAsync("api/FileResources/upload", content);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Response<string>>();
        }

        var error = await response.Content.ReadAsStringAsync();
        return new Response<string> { Success = false, Message = error };
    }

    public async Task<Stream> DownloadFile(Guid publicId)
    {
        var response = await _httpClient.GetAsync($"api/FileResources/download/{publicId}");
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStreamAsync();
        }

        throw new Exception($"Failed to download file. Status: {response.StatusCode}");
    }
}
