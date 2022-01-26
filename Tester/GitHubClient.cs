namespace Tester
{
    using RestSharp;
    using System.IO.Compression;

    public class GitHubClient
    {
        readonly RestClient _client;

        /// <summary>
        /// Конструктор для создания клиента.
        /// </summary>
        public GitHubClient()
        {
            _client = new RestClient("https://api.github.com/")
                .AddDefaultHeader(KnownHeaders.Accept, "application/vnd.github.v3+json");
        }

        public async Task<string> DownloadRepoAsync(string gitZipUri)
        {
            var tempFolder = Path.Combine(Path.GetTempPath(),$"Repo_{Guid.NewGuid()}");
            try
            {
                int uriIndex = gitZipUri.IndexOf('m');
                gitZipUri = gitZipUri.Remove(0, uriIndex + 1);
                gitZipUri = "https://api.github.com/repos" + gitZipUri + "/zipball";

                var repoBytes = await _client.DownloadDataAsync(new RestRequest(gitZipUri, Method.Get));
                await File.WriteAllBytesAsync(tempFolder + "repo.zip", repoBytes);
                ZipFile.ExtractToDirectory(tempFolder + "repo.zip", tempFolder);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return tempFolder;
        }
    }
}