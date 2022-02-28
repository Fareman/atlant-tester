namespace Tester.ResponseObjects.ReportItems;

public record PostmanStage
{
    public string Description { get; init; } = null!;

    public StatusCode Result { get; set; } = StatusCode.Error;
}