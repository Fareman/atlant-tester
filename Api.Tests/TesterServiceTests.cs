namespace Api.Tests;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Tester;
using Tester.ResponseObjects.ReportItems;

public class TesterServiceTests
{
    private readonly Mock<ILogger<TesterService>> _mockLogger = new();

    private readonly Mock<IGitHubClient> _mockClient = new();

    private readonly TesterService _testerService;

    private readonly string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));

    public TesterServiceTests()
    {
        _testerService = new TesterService(_mockClient.Object, _mockLogger.Object);
    }

    [Test]
    public async Task CreateBuildAsync_InvalidCall()
    {
        var found = true;
        string[] errors =
        {
            "CS1003", "CS1002", "CS1022"
        };

        var tempFolder = Path.Combine(path, @"StageTestingProjects\ProjectFailedBuild");

        //Act
        var actual = await _testerService.CreateBuildAsync(tempFolder);

        foreach (var s in errors)
        {
            if (!actual.Description.Contains(s))
                found = false;
        }

        //Assert
        Assert.IsTrue(found);
    }

    [Test]
    public async Task CreateBuildAsync_ValidCall()
    {
        //Arrange
        var tempFolder = Directory.GetDirectories(path, "ProjectSuccessfulBuild", SearchOption.AllDirectories).First();
        var expected = new BuildStage {Result = StatusCode.Ok, Description = "Successful build"};

        //Act
        var actual = await _testerService.CreateBuildAsync(tempFolder);

        //Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public async Task ExecTestsAsync_DockerError()
    {
        //Arrange
        var tempFolder = Path.Combine(path, @"StageTestingProjects\ContainerFailedBuild");
        var expectedDescription = Path.Combine(tempFolder, "error.txt");
        var description = await File.ReadAllTextAsync(expectedDescription);

        var expected = new PostmanStage {Result = StatusCode.Error, Description = $"{description}\r\n"};

        //Act
        var actual = await _testerService.ExecTestsAsync(tempFolder);

        //Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public async Task ExecTestsAsync_InvalidPostmanError()
    {
        //Arrange
        var tempFolder = Path.Combine(path, @"StageTestingProjects\PostmanError");
        var expectedDescription = Path.Combine(tempFolder, "error.txt");
        var description = await File.ReadAllTextAsync(expectedDescription);

        var expected = new PostmanStage {Result = StatusCode.Error, Description = $"{description}\r\n"};

        //Act
        var actual = await _testerService.ExecTestsAsync(tempFolder);

        //Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public async Task ReshareperStage_InvalidCall()
    {
        //Arrange
        var tempFolder = Path.Combine(path, @"StageTestingProjects\ResharperInvalidProject");
        var expectedDescription = Path.Combine(tempFolder, "error.txt");
        var description = await File.ReadAllTextAsync(expectedDescription);

        var expected = new ResharperStage {Result = StatusCode.Error, Description = $"{description}"};

        //Act
        var actual = await _testerService.ExecResharperAsync(tempFolder);

        //Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public async Task ReshareperStage_ValidCall()
    {
        //Arrange
        var tempFolder = Path.Combine(path, @"StageTestingProjects\ResharperValidProject");
        var expectedDescription = Path.Combine(tempFolder, "error.txt");
        var description = await File.ReadAllTextAsync(expectedDescription);

        var expected = new ResharperStage {Result = StatusCode.Ok, Description = $"{description}"};

        //Act
        var actual = await _testerService.ExecResharperAsync(tempFolder);

        //Assert
        Assert.AreEqual(expected, actual);
    }
}