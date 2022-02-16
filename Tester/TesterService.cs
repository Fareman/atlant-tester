namespace Tester;

using System.Diagnostics;
using System.Text;

using CliWrap;
using static Tester.ResponseObjects.ReportTemplate;

public class TesterService
{
    private readonly IGitHubClient _client;

    public TesterService(IGitHubClient client)
    {
        _client = client;
    }

    public async Task<Report> CreateBuildAsync(string tempFolder)
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
            return new Report { BuildStage = { Result = StatusCode.Ok, Description = stdOutBuffer.ToString()} };
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

    public async Task<Report> ExecResharperAsync(string tempFolder)
    {
        var slnPath = Directory.GetFiles(tempFolder, "*.sln", SearchOption.AllDirectories).First();
        var stdOutBuffer = new StringBuilder();

        var resharperCommand = Cli.Wrap("jb")
                                  .WithArguments($"inspectcode {slnPath} --output=REPORT.xml")
                                  .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                                  .WithWorkingDirectory(tempFolder)
                                  .ExecuteAsync();
        await resharperCommand;

        return new Report { ResharperStage = { Result = StatusCode.Ok, Description = stdOutBuffer.ToString() } };
    }

    public async Task<Report> ExecTestsAsync(string tempFolder)
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\"));

        try
        {
            var stdOutBuffer = new StringBuilder();
            var stdErrBuffer = new StringBuilder();

            var testProjectComposeFile = GetDockerComposeFile(path);
            var serviceProjectComposeFile = GetDockerComposeFile(tempFolder);

            var dockerCommand = Cli.Wrap("docker-compose")
                                   .WithArguments(
                                       $"-f {testProjectComposeFile} -f {serviceProjectComposeFile} up --abort-on-container-exit")
                                   .WithWorkingDirectory(tempFolder)
                                   .WithValidation(CommandResultValidation.None)
                                  .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                                   .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                                   .ExecuteAsync();
            await dockerCommand;

            return new Report { PostmanStage = { Result = StatusCode.Ok, Description = stdOutBuffer.ToString() } };
        }
        finally
        {
            Directory.Delete(tempFolder, true);
        }
    }

    public async Task<string> MakeReportAsync(Report buildReport, Report resharperReport, Report postamanReport)
    {
        throw new NotSupportedException();
    }

    public async Task<string> TestAsync(string gitUri)
    {
        var tempFolder = await _client.DownloadRepoAsync(gitUri);
        var buildReport = await CreateBuildAsync(tempFolder);
        if (buildReport.BuildStage.Result == StatusCode.Ok)
        {
            var resharperReport = await ExecResharperAsync(tempFolder);
            var postamanReport = await ExecTestsAsync(tempFolder);
            var report = await MakeReportAsync(buildReport, resharperReport, postamanReport);
            return report;
        }

        return buildReport.BuildStage.Description;
    }

    private static string GetDockerComposeFile(string baseDirectory)
    {
        var path = Directory.GetFiles(baseDirectory, "docker-compose.yml", SearchOption.AllDirectories).First();
        return path;
    }
}