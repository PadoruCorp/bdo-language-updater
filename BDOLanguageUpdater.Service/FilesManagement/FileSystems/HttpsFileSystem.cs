using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using BDOLanguageUpdater.Service;
using Microsoft.Extensions.Logging;

namespace Padoru.Core.Files;

public class HttpsFileSystem : IFileSystem
{
    private readonly ILogger<HttpsFileSystem> logger;
    private readonly HttpClient httpClient;

    public HttpsFileSystem(IHttpClientFactory httpClientFactory, ILogger<HttpsFileSystem> logger)
    {
        this.logger = logger;
        this.httpClient = httpClientFactory.CreateClient(Constants.HTTP_CLIENT_NAME);
    }

    public async Task<bool> Exists(string uri)
    {
        var path = FileUtils.PathFromUri(uri);
        var requestUri = "https://" + path;
        var response = await this.httpClient.GetAsync(requestUri);
        return response.IsSuccessStatusCode;
    }

    public async Task<File<byte[]>> Read(string uri)
    {
        var path = FileUtils.PathFromUri(uri);
        var requestUri = "https://" + path;
        var response = await this.httpClient.GetAsync(requestUri);

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsByteArrayAsync();
            this.logger.LogTrace($"Read file at path '{requestUri}'.");
            return new File<byte[]>(uri, data);
        }

        throw new FileNotFoundException($"Could not read file at path '{uri}'. Error code: {response.StatusCode}");
    }

    public async Task Write(File<byte[]> file)
    {
        var path = FileUtils.PathFromUri(file.Uri);
        var requestUri = "https://" + path;
        var content = new ByteArrayContent(file.Data);
        var response = await this.httpClient.PostAsync(requestUri, content);

        if (response.IsSuccessStatusCode)
        {
            this.logger.LogTrace($"Written file at path '{file.Uri}'.");
        }
        else
        {
            this.logger.LogError($"Could not write file at path '{file.Uri}'. Error code: {response.StatusCode}");
        }
    }

    public async Task Delete(string uri)
    {
        var path = FileUtils.PathFromUri(uri);
        var requestUri = "https://" + path;
        var response = await this.httpClient.DeleteAsync(requestUri);

        if (response.IsSuccessStatusCode)
        {
            this.logger.LogTrace($"Deleted file at path '{uri}'.");
            return;
        }

        throw new FileNotFoundException($"Could not find file. Uri {uri}. Error code: {response.StatusCode}");
    }
}