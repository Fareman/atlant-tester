namespace Tester.ResponseObjects.ReportItems;

public record ContainerStage
{
    public string Description { get; init; } = null!;

    public StatusCode Result { get; set; }
}