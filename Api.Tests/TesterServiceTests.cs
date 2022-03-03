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
    private readonly Mock<IGitHubClient> _mockClient = new();

    private readonly Mock<ILogger<TesterService>> _mockLogger = new();

    private readonly string _path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));

    private readonly TesterService _testerService;

    public TesterServiceTests()
    {
        _testerService = new TesterService(_mockClient.Object, _mockLogger.Object);
    }

    [Test]
    public async Task CreateBuildAsync_InvalidCall()
    {
        //Arrange
        var errorCodesNotFound = false;
        string[] errors =
        {
            "CS0116", "CS0118", "CS0246", "CS0538", "CS0501"
        };

        var tempFolder = Path.Combine(_path, @"StageTestingProjects\ProjectFailedBuild");

        //Act
        var actual = await _testerService.CreateBuildAsync(tempFolder);

        foreach (var s in errors)
        {
            if (!actual.Description.Contains(s))
                errorCodesNotFound = true;
        }

        //Assert
        Assert.IsFalse(errorCodesNotFound);
    }

    [Test]
    public async Task CreateBuildAsync_ValidCall()
    {
        //Arrange
        var tempFolder = Directory.GetDirectories(_path, "ProjectSuccessfulBuild", SearchOption.AllDirectories).First();
        var expected = new BuildStage { Result = StatusCode.Ok, Description = "������ ��������� �������." };

        //Act
        var actual = await _testerService.CreateBuildAsync(tempFolder);

        //Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public async Task ExecTestsAsync_DockerError()
    {
        //Arrange
        var tempFolder = Path.Combine(_path, @"StageTestingProjects\ContainerFailedBuild");
        var expectedDescription = Path.Combine(tempFolder, "error.txt");
        var description = await File.ReadAllTextAsync(expectedDescription);

        var expected = new PostmanStage { Result = StatusCode.Error, Description = $"{description}" };

        //Act
        var actual = await _testerService.ExecTestsAsync(tempFolder);

        //Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public async Task ExecTestsAsync_InvalidPostmanError()
    {
        //Arrange
        var reportPartsNotFound = false;
        string[] errors =
        {
            "PostPerson", "tests=\"1\" failures=\"1\"",
            "GetPersons",
            "GetPersonId",
            "DeletePersonId",
            "DeletePersonId"
        };

        var tempFolder = Path.Combine(_path, @"StageTestingProjects\PostmanError");

        //Act
        var actual = await _testerService.ExecTestsAsync(tempFolder);

        foreach (var s in errors)
        {
            if (!actual.Description.Contains(s))
                reportPartsNotFound = true;
        }

        //Assert
        Assert.IsFalse(reportPartsNotFound);
    }

    [Test]
    public async Task Postman_ValidCall()
    {
        //Arrange
        var reportPartsNotFound = false;
        string[] errors =
        {
            "PostPerson", "tests=\"1\" failures=\"1\"",
            "GetPersons",
            "GetPersonId",
            "DeletePersonId",
            "DeletePersonId"
        };

        var tempFolder = Path.Combine(_path, @"StageTestingProjects\PostmanRealExample");

        //Act
        var actual = await _testerService.ExecTestsAsync(tempFolder);

        foreach (var s in errors)
        {
            if (!actual.Description.Contains(s))
                reportPartsNotFound = true;
        }

        //Assert
        Assert.IsFalse(reportPartsNotFound);
    }

    [Test]
    public async Task ReshareperStage_InvalidCall()
    {
        //Arrange
        var tempFolder = Path.Combine(_path, @"StageTestingProjects\ResharperInvalidProject");
        var expectedDescription = Path.Combine(tempFolder, "error.txt");
        var description = await File.ReadAllTextAsync(expectedDescription);

        var expected = new ResharperStage { Result = StatusCode.Error, Description = $"{description}" };

        //Act
        var actual = await _testerService.ExecResharperAsync(tempFolder);

        //Assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public async Task ReshareperStage_ValidCall()
    {
        //Arrange
        var tempFolder = Path.Combine(_path, @"StageTestingProjects\ResharperValidProject");
        var expectedDescription = Path.Combine(tempFolder, "error.txt");
        var description = await File.ReadAllTextAsync(expectedDescription);

        var expected = new ResharperStage { Result = StatusCode.Ok, Description = $"{description}" };

        //Act
        var actual = await _testerService.ExecResharperAsync(tempFolder);

        //Assert
        Assert.AreEqual(expected, actual);
    }
}