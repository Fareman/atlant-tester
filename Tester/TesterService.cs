namespace Tester;

using System.Text;
using System.Xml;
using System.Xml.Linq;

using CliWrap;

using Microsoft.Extensions.Logging;

using Tester.ResponseObjects;
using Tester.ResponseObjects.ReportItems;

public class TesterService
{
    private static readonly string _slnName = "*.sln";

    private readonly IGitHubClient _client;

    private readonly ILogger<TesterService> _logger;

    public TesterService(IGitHubClient client, ILogger<TesterService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<BuildStage> CreateBuildAsync(string tempFolder)
    {
        var stdOutBuffer = new StringBuilder();

        try
        {
            var slnPath = FindSln(tempFolder);
            var workingDirectory = Path.GetDirectoryName(slnPath);

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
        catch (Exception ex)
        {
            _logger.LogError("Dotnet command threw an exception.", ex);
            return new BuildStage {Result = StatusCode.Exception, Description = ex.Message};
        }
    }

    public async Task<ResharperStage> ExecResharperAsync(string tempFolder)
    {
        var stdOutBuffer = new StringBuilder();

        try
        {
            var slnPath = FindSln(tempFolder);
            var codeStyle = Path.Combine(Directory.GetCurrentDirectory(), "codestyle.DotSettings");

            await Cli.Wrap("jb")
                     .WithArguments($"inspectcode {slnPath} --output=REPORT.xml --profile={codeStyle}")
                     .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                     .WithWorkingDirectory(tempFolder)
                     .ExecuteAsync();

            var xmlPath = Path.Combine(tempFolder, "REPORT.xml");
            var xmlFile = File.ReadAllText($"{xmlPath}");
            var xmldoc = new XmlDocument();
            xmldoc.LoadXml(xmlFile);

            var hasProjectIssues = false;
            foreach (XmlNode node in xmldoc.DocumentElement.ChildNodes)
            {
                if (node.Name == "IssueTypes")
                    hasProjectIssues = node.HasChildNodes;
            }

            if (hasProjectIssues)
                return new ResharperStage {Result = StatusCode.Error, Description = xmlFile};
            return new ResharperStage {Result = StatusCode.Ok, Description = xmlFile};
        }
        catch (Exception ex)
        {
            _logger.LogError("Resharper command threw an exception.", ex);
            return new ResharperStage {Result = StatusCode.Exception, Description = ex.Message};
        }
    }

    public async Task<PostmanStage> ExecTestsAsync(string tempFolder)
    {
        var stdErrBuffer = new StringBuilder();

        var testProjectComposeFile =
            Directory.GetFiles(tempFolder, "docker-compose.yml", SearchOption.AllDirectories).SingleOrDefault();
        var serviceComposeFile = Path.Combine(Directory.GetCurrentDirectory(), "docker-compose.yml");

        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\"));

        try
        {
            var dockerCommand = await Cli.Wrap("docker-compose")
                                         .WithArguments(
                                             $"-f {testProjectComposeFile} -f {serviceComposeFile} up --abort-on-container-exit")
                                         .WithWorkingDirectory(tempFolder)
                                         .WithValidation(CommandResultValidation.None)
                                         .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                                         .ExecuteAsync();

            if (dockerCommand.ExitCode == 0)
            {
                var postmanReport = Path.Combine(tempFolder, @"postman\newman-report.xml");
                var xmlDocument = XDocument.Load($"{postmanReport}");
                return new PostmanStage {Result = StatusCode.Ok, Description = xmlDocument.ToString()};
            }

            return new PostmanStage {Result = StatusCode.Error, Description = $"{stdErrBuffer}"};
        }
        catch (Exception ex)
        {
            _logger.LogError("Docker-compose threw an exception.", ex);
            return new PostmanStage {Result = StatusCode.Exception, Description = $"{ex.Message}"};
        }
        finally
        {
            await Cli.Wrap("docker-compose")
                     .WithArguments("rm -f -s")
                     .WithWorkingDirectory(Directory.GetCurrentDirectory())
                     .WithValidation(CommandResultValidation.None)
                     .ExecuteAsync();
            //Directory.Delete(tempFolder, true);
        }
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

    private static string FindSln(string tempFolder)
    {
        return Directory.GetFiles(tempFolder, $"{_slnName}", SearchOption.AllDirectories).SingleOrDefault();
    }
}