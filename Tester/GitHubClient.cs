namespace Tester;

using System.IO.Compression;

using RestSharp;

public class GitHubClient
{
    private readonly RestClient _client;

    /// <summary>
    ///     Конструктор для создания клиента.
    /// </summary>
    public GitHubClient(RestClient client)
    {
        _client = client.AddDefaultHeader(KnownHeaders.Accept, "application/vnd.github.v3+json");
    }

    public async Task<string> DownloadRepoAsync(string gitUrl)
    {
        var tempFolder = Path.Combine(Path.GetTempPath(), $"Repo_{Guid.NewGuid()}");
        var zipFolder = $"{tempFolder}repo.zip";
        var uriIndex = gitUrl.IndexOf('m');
        gitUrl = gitUrl.Remove(0, uriIndex + 1);
        gitUrl = $"https://api.github.com/repos{gitUrl}/zipball";

        var repoBytes = await _client.DownloadDataAsync(new RestRequest(gitUrl));
        await File.WriteAllBytesAsync(zipFolder, repoBytes);
        ZipFile.ExtractToDirectory(zipFolder, tempFolder);
        return tempFolder;
    }
}