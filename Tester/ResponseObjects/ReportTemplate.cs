using Tester.ResponseObjects.ReportItems;

namespace Tester.ResponseObjects;

public class ReportTemplate
{
    public class Report
    {
        public BuildStage BuildStage { get; set; }

        public ContainerStage ContainerStage { get; set; }

        public PostmanStage PostmanStage { get; set; }

        public ResharperStage ResharperStage { get; set; }
    }
}