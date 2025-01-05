namespace UWUVCI_AIO_WPF.Models
{
    public class ToolStep
    {
        public string ToolName { get; set; } // The name of the tool to run (e.g., wit, nfs2iso2nfs)
        public string Arguments { get; set; } // The arguments to pass to the tool
        public string RealPath { get; set; } // The path to a file that actually exists
        public string Function { get; set; } // The function in UWUVCI where the step originated
    }

}