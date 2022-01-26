namespace Tester
{
    using CliWrap;
    using System.Diagnostics;

    public class TesterService: ITesterService
    {
        private readonly GitHubClient _client;

        public TesterService(GitHubClient client)
        {
            _client = client;
        }

        public async Task TestAsync(string gitZipUri)
        {
            //LINK#1 https://api.github.com/repos/YaroslavIngvarsson/HallOfFame/zipball
            //LINK#2 https://api.github.com/repos/Fareman/TestTAP/zipball
            var tempFolder = await _client.DownloadRepoAsync(gitZipUri);
            await CreateBuildAsync(tempFolder);
            await ExecTestsAsync();
            await ExecResharperAsync();
            await MakeReportAsync();
        }

        public async Task CreateBuildAsync(string tempFolder)
        {
            Process appProcess = null;
            Process dotnetProcess = null;
            try
            {
                var workingDirectoryName = Directory.GetDirectories("Repo").First();

                var workingDirectory = "C:\\Users\\HarinMA\\Desktop\\Api\\Api\\Repo\\YaroslavIngvarsson-HallOfFame-f31bb89";



                appProcess = new Process();
                await Cli.Wrap("dotnet")
                    .WithArguments("build")
                    .WithWorkingDirectory(workingDirectory)
                    .ExecuteAsync();

                await Cli.Wrap("dotnet")
                    .WithArguments("run --project HallOfFame.Api.csproj")
                    .WithWorkingDirectory("C:\\Users\\HarinMA\\Desktop\\Api\\Api\\Repo\\YaroslavIngvarsson-HallOfFame-f31bb89\\HallOfFame.Api")
                    .WithValidation(CommandResultValidation.None)
                    .ExecuteAsync();
                await Task.Delay(5000 * 20);
            }
            finally
            {
                try
                {
                    appProcess?.Kill();
                }
                catch
                {
                    // ignored
                }

                try
                {
                    dotnetProcess?.Kill();
                }
                catch
                {
                    // ignored
                }
            }
        }

        public async Task ExecTestsAsync()
        {
            throw new NotSupportedException();
        }

        public async Task ExecResharperAsync()
        {
            throw new NotSupportedException();
        }

        public async Task MakeReportAsync()
        {
            throw new NotSupportedException();
        }
    }
}