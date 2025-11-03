using System;

namespace UWUVCI_AIO_WPF.Helpers
{
    public interface IToolRunnerFacade
    {
        void RunTool(string toolBaseName, string toolsPathWin, string argsWindowsPaths, bool showWindow, string workDirWin = @"C:\\uwu");
        void RunToolWithFallback(string toolBaseName, string toolsPathWin, string argsWindowsPaths, bool showWindow, string workDirWin = @"C:\\uwu");
    }

    public sealed class DefaultToolRunnerFacade : IToolRunnerFacade
    {
        public static readonly DefaultToolRunnerFacade Instance = new DefaultToolRunnerFacade();
        private DefaultToolRunnerFacade() { }

        public void RunTool(string toolBaseName, string toolsPathWin, string argsWindowsPaths, bool showWindow, string workDirWin = @"C:\\uwu")
            => ToolRunner.RunTool(toolBaseName, toolsPathWin, argsWindowsPaths, showWindow, workDirWin);

        public void RunToolWithFallback(string toolBaseName, string toolsPathWin, string argsWindowsPaths, bool showWindow, string workDirWin = @"C:\\uwu")
            => ToolRunner.RunToolWithFallback(toolBaseName, toolsPathWin, argsWindowsPaths, showWindow, workDirWin);
    }
}

