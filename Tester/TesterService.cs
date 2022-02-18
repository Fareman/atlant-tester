namespace Tester;
using CliWrap;
using System.Text;
using System.Xml;
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

            return new BuildStage { Result = StatusCode.Ok, Description = string.Empty };
        else
            return new BuildStage { Result = StatusCode.Error, Description = stdOutBuffer.ToString() };
    }

    public async Task<ResharperStage> ExecResharperAsync(string tempFolder)
    {
        var slnPath = GetFileDirectory(tempFolder, "*.sln");
        var stdOutBuffer = new StringBuilder();

        await Cli.Wrap("jb").WithArguments($"inspectcode {slnPath} --output=REPORT.xml")
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithWorkingDirectory(tempFolder)
            .ExecuteAsync();

        var xmlPath = GetFileDirectory(tempFolder, "REPORT.xml");
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xmlPath);
             
        XmlElement element = xmlDocument.DocumentElement;
        var output = string.Empty;
        foreach (XmlNode xnode in element)
        {
            if (xnode.Attributes.Count > 0)
                output= xnode.Value;
            foreach(XmlNode ynode in xnode.ChildNodes)
            {
                output= ynode.Value;
            }
        }

        return new ResharperStage { Result = StatusCode.Ok, Description = xmlDocument.Value };
    }

    public async Task<PostmanStage> ExecTestsAsync(string tempFolder)
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\"));

        try
        {
            var stdOutBuffer = new StringBuilder();
            var stdErrBuffer = new StringBuilder();

            var testProjectComposeFile = GetFileDirectory(path, "docker-compose.yml");
            var serviceProjectComposeFile = GetFileDirectory(tempFolder, "docker-compose.yml");

            var dockerCommand = Cli.Wrap("docker-compose")
                                   .WithArguments(
                                       $"-f {testProjectComposeFile} -f {serviceProjectComposeFile} up --abort-on-container-exit")
                                   .WithWorkingDirectory(tempFolder)
                                   .WithValidation(CommandResultValidation.None)
                                   .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                                   .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                                   .ExecuteAsync();
            await dockerCommand;

            return new PostmanStage { Result = StatusCode.Ok, Description = stdOutBuffer.ToString()};
        }
        finally
        {
            //Directory.Delete(tempFolder, true);
        }
    }

    public static Report MakeReportAsync(BuildStage buildReport, ResharperStage resharperReport, PostmanStage postamanReport)
    {
        return new Report { BuildStage = buildReport, ResharperStage = resharperReport, ContainerStage = new ContainerStage { Result = StatusCode.Ok, Description = string.Empty}, PostmanStage = postamanReport };
    }

    public async Task<Report> TestAsync(string gitUri)
    {
        var tempFolder = await _client.DownloadRepoAsync(gitUri);
        var buildReport = await CreateBuildAsync(tempFolder);
        if (buildReport.Result == StatusCode.Ok)
        {
            var resharperReport = await ExecResharperAsync(tempFolder);
            var postamanReport = await ExecTestsAsync(tempFolder);
            var report = MakeReportAsync(buildReport, resharperReport, postamanReport);
            return report;
        }

        return new Report { BuildStage = buildReport };
    }

    private static string GetFileDirectory(string baseDirectory, string file)
    {
        var path = Directory.GetFiles(baseDirectory, $"{file}", SearchOption.AllDirectories).First();
        return path;
    }
}