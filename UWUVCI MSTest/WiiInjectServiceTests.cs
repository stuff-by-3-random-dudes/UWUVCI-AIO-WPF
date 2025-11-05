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
    public class WiiInjectServiceTests
    {
        private sealed class FakeRunner : IToolRunnerFacade
        {
            public readonly List<(string tool, string path, string args, bool show, string workdir)> Calls
                = new List<(string, string, string, bool, string)>();

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
                    var m = Regex.Match(args, @"--dest\s+""([^""]+)""");
                    if (m.Success)
                    {
                        var dest = m.Groups[1].Value;
                        Directory.CreateDirectory(Path.GetDirectoryName(dest));
                        File.WriteAllBytes(dest, new byte[] { 0x41, 0x42, 0x43, 0x44, 0x45 });
                    }
                }
                else if (args.Contains(" extract "))
                {
                    var mDest = Regex.Match(args, @"--DEST\s+""([^""]+)""");
                    if (mDest.Success)
                    {
                        var outDir = mDest.Groups[1].Value;
                        Directory.CreateDirectory(outDir);
                        var sys = Path.Combine(outDir, "DATA", "sys");
                        Directory.CreateDirectory(sys);
                        var dol = Path.Combine(sys, "main.dol");
                        if (!File.Exists(dol)) File.WriteAllBytes(dol, new byte[] { 0 });

                        File.WriteAllBytes(Path.Combine(outDir, "tmd.bin"), new byte[] { 1 });
                        File.WriteAllBytes(Path.Combine(outDir, "ticket.bin"), new byte[] { 2 });
                    }
                }
            }
        }

        [TestMethod]
        public void InjectStandard_BuildsExpectedWitAndNfsCalls()
        {
            var tmp = Path.Combine(Path.GetTempPath(), "uwu_wii_std_" + Guid.NewGuid().ToString("N"));
            var tools = Path.Combine(tmp, "tools");
            var tempPath = Path.Combine(tmp, "temp");
            var basePath = Path.Combine(tmp, "base");
            Directory.CreateDirectory(tools);
            Directory.CreateDirectory(tempPath);
            Directory.CreateDirectory(basePath);
            Directory.CreateDirectory(Path.Combine(basePath, "content"));
            Directory.CreateDirectory(Path.Combine(basePath, "code"));
            Directory.CreateDirectory(Path.Combine(basePath, "meta"));

            File.WriteAllText(Path.Combine(basePath, "meta", "meta.xml"), "<menu><reserved_flag2>deadbeef</reserved_flag2></menu>");
            File.WriteAllBytes(Path.Combine(tools, "nfs2iso2nfs.exe"), new byte[] { 0 });

            var rom = Path.Combine(tmp, "in.iso");
            File.WriteAllBytes(rom, new byte[] { 0x47, 0x43, 0x49, 0x44, 0x00, 0x00 });

            var runner = new FakeRunner();
            var opt = new WiiInjectOptions { Debug = false, PatchVideo = false, DontTrim = false, Index = 0, LR = false };
            WiiInjectService.InjectStandard(tools, tempPath, basePath, rom, opt, runner);

            Assert.IsTrue(runner.Calls.Any(c => c.tool == "wit" && c.args.Contains(" copy ")), "wit copy not invoked");
            Assert.IsTrue(runner.Calls.Any(c => c.tool == "wit" && c.args.Contains(" extract ")), "wit extract not invoked");
            Assert.IsTrue(runner.Calls.Any(c => c.tool == "nfs2iso2nfs"), "nfs2iso2nfs not invoked");
        }
    }
}


