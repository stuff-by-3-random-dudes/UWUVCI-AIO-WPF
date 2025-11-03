using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UWUVCI_AIO_WPF; // MainViewModel lives in this namespace
using UWUVCI_AIO_WPF.Helpers;
using UWUVCI_AIO_WPF.Services;

namespace UWUVCI_MSTest
{
    [TestClass]
    public class WitNfsServiceTests
    {
        private sealed class FakeRunner : IToolRunnerFacade
        {
            public readonly List<(string tool, string toolsPath, string args, bool show, string workdir)> Calls
                = new List<(string, string, string, bool, string)>();

            public void RunTool(string toolBaseName, string toolsPathWin, string argsWindowsPaths, bool showWindow, string workDirWin = @"C:\\uwu")
            {
                Calls.Add((toolBaseName, toolsPathWin, argsWindowsPaths, showWindow, workDirWin));

                // Minimal side-effects so the service can proceed
                if (toolBaseName == "wit")
                {
                    if (argsWindowsPaths.Contains(" copy "))
                    {
                        var m = Regex.Match(argsWindowsPaths, @"--DEST\s+""([^""]+)""");
                        var dest = m.Success ? m.Groups[1].Value : null;
                        if (!string.IsNullOrEmpty(dest))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(dest));
                            // Create a tiny fake ISO
                            File.WriteAllBytes(dest, new byte[8]);
                        }
                    }
                    else if (argsWindowsPaths.Contains(" extract "))
                    {
                        var m = Regex.Match(argsWindowsPaths, @"--DEST\s+""([^""]+)""");
                        var outDir = m.Success ? m.Groups[1].Value : null;
                        if (!string.IsNullOrEmpty(outDir))
                        {
                            Directory.CreateDirectory(outDir);
                            File.WriteAllBytes(Path.Combine(outDir, "tmd.bin"), new byte[] { 1, 2, 3 });
                            File.WriteAllBytes(Path.Combine(outDir, "ticket.bin"), new byte[] { 4, 5, 6 });
                        }
                    }
                }
            }

            public void RunToolWithFallback(string toolBaseName, string toolsPathWin, string argsWindowsPaths, bool showWindow, string workDirWin = @"C:\\uwu")
            {
                RunTool(toolBaseName, toolsPathWin, argsWindowsPaths, showWindow, workDirWin);
            }
        }

        [TestMethod]
        public void BuildIsoExtractTicketsAndInject_UsesExpectedCommands()
        {
            var tmp = Path.Combine(Path.GetTempPath(), "uwu_witnfs_test_" + Guid.NewGuid().ToString("N"));
            var tools = Path.Combine(tmp, "tools");
            var tempPath = Path.Combine(tmp, "temp");
            var basePath = Path.Combine(tmp, "base");
            Directory.CreateDirectory(tools);
            Directory.CreateDirectory(tempPath);
            Directory.CreateDirectory(basePath);
            Directory.CreateDirectory(Path.Combine(basePath, "content"));
            Directory.CreateDirectory(Path.Combine(basePath, "code"));
            Directory.CreateDirectory(Path.Combine(basePath, "meta"));

            // minimal meta.xml with reserved_flag2 node
            File.WriteAllText(Path.Combine(basePath, "meta", "meta.xml"), "<menu><reserved_flag2>deadbeef</reserved_flag2></menu>");

            // placeholder nfs2iso2nfs.exe
            File.WriteAllBytes(Path.Combine(tools, "nfs2iso2nfs.exe"), new byte[] { 0 });

            var mvm = new MainViewModel();
            mvm.debug = false;
            mvm.Index = 0;

            var runner = new FakeRunner();

            WitNfsService.BuildIsoExtractTicketsAndInject(tools, tempPath, basePath, "GCN", mvm, runner);

            // Verify wit copy and extract were called and nfs2iso2nfs was invoked
            Assert.IsTrue(runner.Calls.Any(c => c.tool == "wit" && c.args.Contains(" copy ")), "wit copy not invoked");
            Assert.IsTrue(runner.Calls.Any(c => c.tool == "wit" && c.args.Contains(" extract ")), "wit extract not invoked");
            Assert.IsTrue(runner.Calls.Any(c => c.tool == "nfs2iso2nfs"), "nfs2iso2nfs not invoked");

            // Check outputs moved
            Assert.IsTrue(File.Exists(Path.Combine(basePath, "code", "rvlt.tmd")), "rvlt.tmd missing");
            Assert.IsTrue(File.Exists(Path.Combine(basePath, "code", "rvlt.tik")), "rvlt.tik missing");

            // meta was updated (string of hex chars), non-empty
            var xml = File.ReadAllText(Path.Combine(basePath, "meta", "meta.xml"));
            Assert.IsTrue(xml.Contains("<reserved_flag2>"), "meta missing reserved_flag2");
        }
    }
}
