namespace Tester;

public class ReportTemplate
{
    public class BuildStage
    {
        public string Description { get; set; }

        public string Status { get; set; }
    }

    public class ContainerStage
    {
        public string Description { get; set; }

        public string Status { get; set; }
    }

    public class ResharperStage
    {
        public string Description { get; set; }

        public string Status { get; set; }
    }

    public class PostmanStage
    {
        public string Description { get; set; }

        public string Status { get; set; }
    }

    public class Report
    {
        public BuildStage BuildStage { get; set; }

        public ContainerStage ContainerStage { get; set; }

        public PostmanStage PostmanStage { get; set; }

        public ResharperStage ResharperStage { get; set; }
    }

    public class Root
    {
        public ReportTemplate Report { get; set; }
    }
}