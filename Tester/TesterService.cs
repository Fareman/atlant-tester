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
    private readonly IGitHubClient _client;

    private readonly ILogger<TesterService> _logger;

    public TesterService(IGitHubClient client, ILogger<TesterService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<BuildStage> CreateBuildAsync(string tempFolder)
    {
        try
        {
            var slnPath = FindSln(tempFolder);
            if (slnPath == default)
                return new BuildStage { Result = StatusCode.Exception, Description = $"В директории {tempFolder} отсутствует решение TestTAP.sln." };
            var workingDirectory = Path.GetDirectoryName(slnPath);

            var stdOutBuffer = new StringBuilder();

            var dotnetCommand = await Cli.Wrap("dotnet")
                                         .WithArguments($"build {slnPath}")
                                         .WithWorkingDirectory(workingDirectory)
                                         .WithValidation(CommandResultValidation.None)
                                         .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                                         .ExecuteAsync();

            if (dotnetCommand.ExitCode == 0)
                return new BuildStage { Result = StatusCode.Ok, Description = "Сборка завершена успешно." };
            return new BuildStage { Result = StatusCode.Error, Description = stdOutBuffer.ToString() };
        }
        catch (Exception ex)
        {
            _logger.LogError("Найдено исключение на стадии сборки.", ex);
            return new BuildStage { Result = StatusCode.Exception, Description = ex.Message };
        }
    }

    public async Task<ResharperStage> ExecResharperAsync(string tempFolder)
    {
        try
        {
            const string xmlName = "REPORT.xml";
            var slnPath = FindSln(tempFolder);
            if (slnPath == default)
                return new ResharperStage { Result = StatusCode.Exception, Description = $"В директории {tempFolder} отсутствует решение TestTAP.sln." };
            var codeStyle = Path.Combine(Directory.GetCurrentDirectory(), "codestyle.DotSettings");
            var stdOutBuffer = new StringBuilder();

            await Cli.Wrap("jb")
                     .WithArguments($"inspectcode {slnPath} --output={xmlName} --profile={codeStyle}")
                     .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                     .WithWorkingDirectory(tempFolder)
                     .ExecuteAsync();

            var xmlPath = Path.Combine(tempFolder, xmlName);
            if (!File.Exists(xmlPath))
                return new ResharperStage
                    { Result = StatusCode.Exception, Description = $"В директории {tempFolder} нет файла {xmlPath}." };
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
                return new ResharperStage { Result = StatusCode.Error, Description = xmlFile };
            return new ResharperStage { Result = StatusCode.Ok, Description = xmlFile };
        }
        catch (Exception ex)
        {
            _logger.LogError("Resharper command threw an exception.", ex);
            return new ResharperStage { Result = StatusCode.Exception, Description = ex.Message };
        }
    }

    public async Task<PostmanStage> ExecTestsAsync(string tempFolder)
    {
        const string testProjectComposeName = "docker-compose.yml";
        const string serviceComposeName = "docker-compose.yml";

        var stdErrBuffer = new StringBuilder();

        var testProjectComposeFile =
            Directory.GetFiles(tempFolder, testProjectComposeName, SearchOption.AllDirectories).SingleOrDefault();

        if (testProjectComposeFile == default)
            return new PostmanStage
                { Result = StatusCode.Exception, Description = $"В директории {tempFolder} нет файла {testProjectComposeName}." };

        var serviceComposeFile = Path.Combine(AppContext.BaseDirectory, serviceComposeName);

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
                const string postmanReportDirectory = @"TestTAP\postman\newman-report.xml";
                var projectRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\"));
                var reportPath = Path.Combine(tempFolder, postmanReportDirectory);
                var xmlDocument = XDocument.Load($"{reportPath}");
                return new PostmanStage { Result = StatusCode.Ok, Description = xmlDocument.ToString() };
            }

            return new PostmanStage { Result = StatusCode.Error, Description = $"{stdErrBuffer}" };
        }
        catch (Exception ex)
        {
            _logger.LogError("Docker-compose threw an exception.", ex);
            return new PostmanStage { Result = StatusCode.Exception, Description = $"{ex.Message}" };
        }
        finally
        {
            await Cli.Wrap("docker-compose")
                     .WithArguments($"-f {testProjectComposeFile} -f {serviceComposeFile} down --volumes")
                     .WithWorkingDirectory(tempFolder)
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
        if (buildReport.Result != StatusCode.Ok)
            return new Report { BuildStage = buildReport };
        var resharperReport = await ExecResharperAsync(tempFolder);
        var postamanReport = await ExecTestsAsync(tempFolder);
        var report = MakeReport(buildReport, resharperReport, postamanReport);
        return report;
    }

    private static string FindSln(string tempFolder)
    {
        const string slnName = "TestTAP.sln";

        return Directory.GetFiles(tempFolder, slnName, SearchOption.AllDirectories).SingleOrDefault();
    }
}