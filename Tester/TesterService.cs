namespace Tester;

using System.Diagnostics;
using System.Text;

using CliWrap;

public class TesterService
{
    private readonly IGitHubClient _client;

    public TesterService(IGitHubClient client)
    {
        _client = client;
    }

    public async Task<string> CreateBuildAsync(string tempFolder)
    {
        var dotnetProcessId = 0;
        var workingDirectory = Directory.GetDirectories(tempFolder, Path.GetDirectoryName("*.sln"), SearchOption.AllDirectories)
                                        .First();
        try
        {
            var stdOutBuffer = new StringBuilder();

            var dotnetCommand = Cli.Wrap("dotnet")
                                   .WithArguments("build")
                                   .WithWorkingDirectory(workingDirectory)
                                   .WithValidation(CommandResultValidation.None)
                                   .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                                   .ExecuteAsync();

            dotnetProcessId = dotnetCommand.ProcessId;
            await dotnetCommand;
            return stdOutBuffer.ToString();
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

            //Directory.Delete(tempFolder, true);
        }
    }

    public async Task<string> ExecResharperAsync(string tempFolder)
    {
        var slnPath = Directory.GetFiles(tempFolder, "*.sln", SearchOption.AllDirectories).First();

        var resharperCommand = Cli.Wrap("jb")
                                  .WithArguments($"inspectcode {slnPath} --output=REPORT.xml")
                                  .WithWorkingDirectory(tempFolder)
                                  .ExecuteAsync();
        await resharperCommand;

        return resharperCommand.Task.Result.ExitCode.ToString();
    }

    public async Task<string> ExecTestsAsync(string tempFolder)
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\"));

        try
        {
            var stdErrBuffer = new StringBuilder();

            var testProjectComposeFile = GetDockerComposeFile(path);
            var serviceProjectComposeFile = GetDockerComposeFile(tempFolder);

            var dockerCommand = Cli.Wrap("docker-compose")
                                   .WithArguments(
                                       $"-f {testProjectComposeFile} -f {serviceProjectComposeFile} up --abort-on-container-exit")
                                   .WithWorkingDirectory(tempFolder)
                                   .WithValidation(CommandResultValidation.None)
                                   .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                                   .ExecuteAsync();
            await dockerCommand;

            return dockerCommand.Task.Result.ExitCode.ToString();
        }
        finally
        {
            Directory.Delete(tempFolder, true);
        }
    }

    public async Task<string> MakeReportAsync(string postman, string resharper)
    {
        throw new NotSupportedException();
    }

    public async Task<string> TestAsync(string gitUri)
    {
        var tempFolder = await _client.DownloadRepoAsync(gitUri);
        var buildResult = await CreateBuildAsync(tempFolder);
        if (buildResult == "0")
        {
            var resharperResult = await ExecResharperAsync(tempFolder);
            var postmanResult = await ExecTestsAsync(tempFolder);
            var report = await MakeReportAsync(postmanResult, resharperResult);
            return report;
        }

        return buildResult;
    }

    private static string GetDockerComposeFile(string baseDirectory)
    {
        var path = Directory.GetFiles(baseDirectory, "docker-compose.yml", SearchOption.AllDirectories).First();
        return path;
    }
}