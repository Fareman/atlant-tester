namespace Tester;

using System.Diagnostics;
using System.Text;
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
                                   .WithValidation(CommandResultValidation.None)
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
        }
    }

    public async Task ExecResharperAsync()
    {
        throw new NotSupportedException();
    }

    public async Task<string> ExecTestsAsync(string tempFolder)
    {
        var compose = Directory.GetFiles(@"C:\back\atlant-tester", "docker-compose.yml", SearchOption.AllDirectories)
                               .First();
        var composeFile = Directory.GetFiles(tempFolder, "docker-compose.yml", SearchOption.AllDirectories).First();
        try
        {
            var stdOutBuffer = new StringBuilder();
            var stdErrBuffer = new StringBuilder();

            var dockerCommand = Cli.Wrap("docker-compose")
                                   .WithArguments($"-f {compose} -f {composeFile} up --abort-on-container-exit")
                                   .WithWorkingDirectory(tempFolder)
                                   .WithValidation(CommandResultValidation.None)
                                   .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                                   .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                                   .ExecuteAsync();
            await dockerCommand;
            
            return dockerCommand.Task.Result.ExitCode != 0 ? stdOutBuffer.ToString() : stdErrBuffer.ToString();
        }
        finally
        {
            Directory.Delete(tempFolder, true);
        }
    }

    public async Task MakeReportAsync()
    {
        throw new NotSupportedException();
    }

    public async Task<string> TestAsync(string gitUri)
    {
        var tempFolder = await _client.DownloadRepoAsync(gitUri);
        var report = await ExecTestsAsync(tempFolder);
        return report;
    }
}