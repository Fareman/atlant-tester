namespace Tester;

using System.Text;
using System.Xml.Linq;

using CliWrap;

using Tester.ResponseObjects;
using Tester.ResponseObjects.ReportItems;

public class TesterService
{
    private readonly IGitHubClient _client;

    public TesterService(IGitHubClient client)
    {
        _client = client;
    }

    public async Task<BuildStage> CreateBuildAsync(string tempFolder)
    {
        var workingDirectory = Directory.GetDirectories(tempFolder, Path.GetDirectoryName("*.sln"), SearchOption.AllDirectories)
                                        .First();
        var slnPath = Directory.GetFiles(tempFolder, "*.sln", SearchOption.AllDirectories).First();

        var stdOutBuffer = new StringBuilder();

        var dotnetCommand = await Cli.Wrap("dotnet")
                                     .WithArguments($"build {slnPath}")
                                     .WithWorkingDirectory(workingDirectory)
                                     .WithValidation(CommandResultValidation.None)
                                     .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                                     .ExecuteAsync();

        if (dotnetCommand.ExitCode == 0)
            return new BuildStage {Result = StatusCode.Ok, Description = "Successful build"};
        return new BuildStage {Result = StatusCode.Error, Description = stdOutBuffer.ToString()};
    }

    public async Task<ResharperStage> ExecResharperAsync(string tempFolder)
    {
        var slnPath = GetFileDirectory(tempFolder, "*.sln");
        var stdOutBuffer = new StringBuilder();

        await Cli.Wrap("jb")
                 .WithArguments($"inspectcode {slnPath} --output=REPORT.xml")
                 .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                 .WithWorkingDirectory(tempFolder)
                 .ExecuteAsync();

        var xmlPath = GetFileDirectory(tempFolder, "REPORT.xml");
        var xmlDocument = XDocument.Load($"{xmlPath}");

        return new ResharperStage {Result = StatusCode.Ok, Description = xmlDocument.ToString()};
    }

    public async Task<PostmanStage> ExecTestsAsync(string tempFolder)
    {
        var stdErrBuffer = new StringBuilder();

        var testProjectComposeFile = GetFileDirectory(tempFolder, "docker-compose.yml");
        var serviceComposeFile = string.Empty;

        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\"));
        var directories = Directory.GetFiles(path, "docker-compose.yml", SearchOption.AllDirectories);
        var str = "atlant-tester";
        foreach (string s in directories)
        {
            if (s.Contains(str))
                serviceComposeFile = s;
        }

        var dockerCommand = await Cli.Wrap("docker-compose")
                                     .WithArguments(
                                         $"-f {testProjectComposeFile} -f {serviceComposeFile} up --abort-on-container-exit")
                                     .WithWorkingDirectory(tempFolder)
                                     .WithValidation(CommandResultValidation.None)
                                     .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                                     .ExecuteAsync();
        
        if (dockerCommand.ExitCode == 0)
        {
            var postmanReport = GetFileDirectory(path, "newman-report.xml");
            var xmlDocument = XDocument.Load($"{postmanReport}");
            return new PostmanStage {Result = StatusCode.Ok, Description = xmlDocument.ToString()};
        }

        return new PostmanStage {Result = StatusCode.Error, Description = stdErrBuffer.ToString()};
    }

    public static Report MakeReport(BuildStage buildReport, ResharperStage resharperReport, PostmanStage postamanReport)
    {
        return new Report
        {
            BuildStage = buildReport,
            ResharperStage = resharperReport,
            PostmanStage = postamanReport
        };
    }

    public async Task<Report> TestAsync(string gitUri)
    {
        var tempFolder = await _client.DownloadRepoAsync(gitUri);
        var buildReport = await CreateBuildAsync(tempFolder);
        if (buildReport.Result == StatusCode.Ok)
        {
            var resharperReport = await ExecResharperAsync(tempFolder);
            var postamanReport = await ExecTestsAsync(tempFolder);
            var report = MakeReport(buildReport, resharperReport, postamanReport);
            return report;
        }

        return new Report {BuildStage = buildReport};
    }

    private static string GetFileDirectory(string baseDirectory, string file)
    {
        var path = Directory.GetFiles(baseDirectory, $"{file}", SearchOption.AllDirectories).First();
        return path;
    }
}