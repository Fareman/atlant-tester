namespace Api.Tests;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

using Tester;
using Tester.ResponseObjects.ReportItems;

public class TesterServiceTests
{
    private readonly Mock<IGitHubClient> _mockClient = new();

    private readonly TesterService _testerService;

    public TesterServiceTests()
    {
        _testerService = new TesterService(_mockClient.Object);
    }

    [Test]
    public async Task CreateBuildAsync_InvalidCall()
    {
        var found = true;
        string[] errors =
        {
            "CS1003", "CS1002", "CS1022"
        };

        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
        var tempFolder = Directory.GetDirectories(path, "ProjectFailedBuild", SearchOption.AllDirectories).First();

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
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
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
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
        var tempFolder = Directory.GetDirectories(path, "ContainerFailedBuild", SearchOption.AllDirectories).First();
        var expectedDescription = GetFileDirectory(tempFolder, "error.txt");
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
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
        var tempFolder = Directory.GetDirectories(path, "PostmanError", SearchOption.AllDirectories).First();
        var expectedDescription = GetFileDirectory(tempFolder, "error.txt");
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
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
        var tempFolder = Directory.GetDirectories(path, "ResharperInvalidProject", SearchOption.AllDirectories).First();
        var expectedDescription = GetFileDirectory(tempFolder, "error.txt");
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
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
        var tempFolder = Directory.GetDirectories(path, "ResharperValidProject", SearchOption.AllDirectories).First();
        var expectedDescription = GetFileDirectory(tempFolder, "error.txt");
        var description = await File.ReadAllTextAsync(expectedDescription);

        var expected = new ResharperStage {Result = StatusCode.Ok, Description = $"{description}"};

        //Act
        var actual = await _testerService.ExecResharperAsync(tempFolder);

        //Assert
        Assert.AreEqual(expected, actual);
    }

    private static string GetFileDirectory(string baseDirectory, string file)
    {
        var path = Directory.GetFiles(baseDirectory, $"{file}", SearchOption.AllDirectories).First();
        return path;
    }
}