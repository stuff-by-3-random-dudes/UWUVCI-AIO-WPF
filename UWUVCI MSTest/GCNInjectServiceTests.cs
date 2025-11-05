using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using UWUVCI_AIO_WPF; // MainViewModel lives in this namespace
using UWUVCI_AIO_WPF.Helpers;
using UWUVCI_AIO_WPF.Services;

namespace UWUVCI_MSTest
{
    [TestClass]
    public class GCNInjectServiceTests
    {
        private sealed class FakeRunner : IToolRunnerFacade
        {
            public readonly List<(string tool, string path, string args, bool show, string workdir)> Calls
                = [];

            public void RunTool(string toolBaseName, string toolsPathWin, string argsWindowsPaths, bool showWindow, string workDirWin = @"C:\\uwu")
            {
                Calls.Add((toolBaseName, toolsPathWin, argsWindowsPaths, showWindow, workDirWin));
                Simulate(toolBaseName, toolsPathWin, argsWindowsPaths);
            }

            public void RunToolWithFallback(string toolBaseName, string toolsPathWin, string argsWindowsPaths, bool showWindow, string workDirWin = @"C:\\uwu")
            {
                Calls.Add((toolBaseName, toolsPathWin, argsWindowsPaths, showWindow, workDirWin));
                Simulate(toolBaseName, toolsPathWin, argsWindowsPaths);
            }

            private static void Simulate(string tool, string toolsPath, string args)
            {
                if (tool != "wit") return;

                if (args.Contains(" copy "))
                {
                    var m = Regex.Match(args, @"--DEST\s+""([^""]+)""");
                    if (m.Success)
                    {
                        var dest = m.Groups[1].Value;
                        Directory.CreateDirectory(Path.GetDirectoryName(dest));
                        // seed first 4 bytes so reserved_flag2 path runs
                        File.WriteAllBytes(dest, new byte[] { 0x47, 0x43, 0x49, 0x44, 0xFF }); // GCID\xFF
                    }
                }
                else if (args.Contains(" extract "))
                {
                    var mDest = Regex.Match(args, @"--DEST\s+""([^""]+)""");
                    if (mDest.Success)
                    {
                        var outDir = mDest.Groups[1].Value;
                        Directory.CreateDirectory(outDir);
                        File.WriteAllBytes(Path.Combine(outDir, "tmd.bin"), new byte[] { 0x01 });
                        File.WriteAllBytes(Path.Combine(outDir, "ticket.bin"), new byte[] { 0x02 });
                    }
                }
            }
        }

        [TestMethod]
        public void InjectGCN_CallsWitAndNfsPipeline()
        {
            var tmp = Path.Combine(Path.GetTempPath(), "uwu_gcn_" + Guid.NewGuid().ToString("N"));
            var tools = Path.Combine(tmp, "tools");
            var tempPath = Path.Combine(tmp, "temp");
            var basePath = Path.Combine(tmp, "base");
            Directory.CreateDirectory(tools);
            Directory.CreateDirectory(tempPath);
            Directory.CreateDirectory(basePath);
            Directory.CreateDirectory(Path.Combine(basePath, "content"));
            Directory.CreateDirectory(Path.Combine(basePath, "code"));
            Directory.CreateDirectory(Path.Combine(basePath, "meta"));

            // Minimal base dir zipped as BASE.zip
            var baseRoot = Path.Combine(tmp, "BASE");
            Directory.CreateDirectory(baseRoot);
            Directory.CreateDirectory(Path.Combine(baseRoot, "sys"));
            Directory.CreateDirectory(Path.Combine(baseRoot, "files"));
            var zipPath = Path.Combine(tools, "BASE.zip");
            ZipFile.CreateFromDirectory(baseRoot, zipPath);

            // Required nintendont dol in tools
            File.WriteAllBytes(Path.Combine(tools, "nintendont.dol"), new byte[] { 0 });

            // Seed meta.xml for reserved_flag2 update
            File.WriteAllText(Path.Combine(basePath, "meta", "meta.xml"), "<menu><reserved_flag2>deadbeef</reserved_flag2></menu>");

            // placeholder nfs2iso2nfs.exe
            File.WriteAllBytes(Path.Combine(tools, "nfs2iso2nfs.exe"), new byte[] { 0 });

            var rom = Path.Combine(tmp, "game.iso");
            File.WriteAllBytes(rom, new byte[] { 0x00 });

            var runner = new FakeRunner();
            var opt = new GcnInjectOptions { Debug = false, DontTrim = false, Disc2Path = null, Force43 = false };
            GCNInjectService.InjectGCN(tools, tempPath, basePath, rom, opt, runner: runner);

            Assert.IsTrue(runner.Calls.Any(c => c.tool == "wit" && c.args.Contains(" copy ")), "wit copy not invoked");
            Assert.IsTrue(runner.Calls.Any(c => c.tool == "wit" && c.args.Contains(" extract ")), "wit extract not invoked");
            Assert.IsTrue(runner.Calls.Any(c => c.tool == "nfs2iso2nfs"), "nfs2iso2nfs not invoked");
        }
    }
}
