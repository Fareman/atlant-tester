namespace Tester.ResponseObjects.ReportItems;

public record ResharperStage
{
    public string Description { get; init; } = null!;

    public StatusCode Result { get; set; }
}