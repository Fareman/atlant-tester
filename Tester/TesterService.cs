namespace Tester;

using System.Diagnostics;

using CliWrap;

public class TesterService
{
    private readonly GitHubClient _client;

    public TesterService(GitHubClient client)
    {
        _client = client;
    }

    public async Task CreateBuildAsync(string tempFolder)
    {
        var dotnetProcessId = 0;
        var appProcessId = 0;

        var build = Directory.GetFiles(tempFolder, "*.csproj", SearchOption.AllDirectories).First();
        var workingDirectory = Directory.GetDirectories(tempFolder, Path.GetDirectoryName("*.sln"), SearchOption.AllDirectories)
                                        .First();
        try
        {
            var dotnetCommand = Cli.Wrap("dotnet")
                                   .WithArguments("build")
                                   .WithWorkingDirectory(workingDirectory)
                                   .ExecuteAsync();

            dotnetProcessId = dotnetCommand.ProcessId;
            await dotnetCommand;

            var appCommand = Cli.Wrap("dotnet")
                                .WithArguments($"run --project {build}")
                                .WithValidation(CommandResultValidation.None)
                                .ExecuteAsync();

            appProcessId = appCommand.ProcessId;
            await appCommand;
        }
        finally
        {
            try
            {
                var dotnetProcess = Process.GetProcessById(dotnetProcessId);
                dotnetProcess?.Kill(true);
            }
            catch
            {
            }

            try
            {
                var appProcess = Process.GetProcessById(appProcessId);
                appProcess?.Kill(true);
            }
            catch
            {
            }

            try
            {
                Directory.Delete(tempFolder, true);
            }
            catch
            {
            }
        }
    }

    public async Task ExecResharperAsync()
    {
        throw new NotSupportedException();
    }

    public async Task ExecTestsAsync()
    {
        throw new NotSupportedException();
    }

    public async Task MakeReportAsync()
    {
        throw new NotSupportedException();
    }

    public async Task TestAsync(string gitUri)
    {
        var tempFolder = await _client.DownloadRepoAsync(gitUri);
        await CreateBuildAsync(tempFolder);
        await ExecTestsAsync();
        await ExecResharperAsync();
        await MakeReportAsync();
    }
}