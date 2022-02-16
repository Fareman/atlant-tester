namespace Tester
{
    public class ReportTemplate
    {
        public class BuildStage
        {
            public string Status { get; set; }
            public string Description { get; set; }
        }

        public class ContainerStage
        {
            public string Status { get; set; }
            public string Description { get; set; }
        }

        public class ResharperStage
        {
            public string Status { get; set; }
            public string Description { get; set; }
        }

        public class PostmanStage
        {
            public string Status { get; set; }
            public string Description { get; set; }
        }

        public class Report
        {
            public BuildStage BuildStage { get; set; }
            public ContainerStage ContainerStage { get; set; }
            public ResharperStage ResharperStage { get; set; }
            public PostmanStage PostmanStage { get; set; }
        }

        public class Root
        {
            public ReportTemplate Report { get; set; }
        }

    }
}
