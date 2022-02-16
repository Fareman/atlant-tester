namespace Tester;

public interface IGitHubClient
{
    Task<string> DownloadRepoAsync(string gitUrl);
}