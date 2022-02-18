namespace Tester.ResponseObjects;

using Tester.ResponseObjects.ReportItems;

public record Report
{
    public BuildStage BuildStage { get; init; } = null!;

    public ContainerStage ContainerStage { get; init; } = null!;

    public PostmanStage PostmanStage { get; init; } = null!;
    public ResharperStage ResharperStage { get; init; } = null!;
}