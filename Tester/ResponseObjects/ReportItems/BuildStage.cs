namespace Tester.ResponseObjects.ReportItems;

public record BuildStage
{
    public string Description { get; init; } = null!;

    public StatusCode Result { get; set; }
}