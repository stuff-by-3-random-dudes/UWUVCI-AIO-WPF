using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UWUVCI_AIO_WPF.Helpers;
using UWUVCI_AIO_WPF.Models;

namespace UWUVCI_AIO_WPF.Services
{
    public static class GctPatcherService
    {
        public static void PatchWiiDolWithGcts(string toolsPath, string mainDolPath, IEnumerable<string> gctPaths, bool debug)
        {
            if (string.IsNullOrWhiteSpace(mainDolPath) || gctPaths == null)
                return;

            // Normalize to Windows paths for ToolRunner
            string ToWin(string p)
            {
                if (string.IsNullOrEmpty(p)) return p ?? string.Empty;
                if (p.Length > 2 && char.IsLetter(p[0]) && p[1] == ':' && (p[2] == '\\' || p[2] == '/'))
                    return p.Replace('/', '\\');
                if (p[0] == '/')
                    return @"Z:\" + p.TrimStart('/').Replace('/', '\\');
                return p.Replace('/', '\\');
            }

            mainDolPath = ToWin(mainDolPath);

            var converted = new List<string>();
            foreach (var path in gctPaths)
            {
                if (string.IsNullOrWhiteSpace(path)) continue;
                var p = path.Trim();
                string outPath = p;
                if (Path.GetExtension(p).Equals(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var (codes, gameId) = GctCode.ParseOcarinaOrDolphinTxtFile(p);
                        outPath = Path.ChangeExtension(p, ".gct");
                        GctCode.WriteGctFile(outPath, codes, gameId);
                        Logger.Log($"Converted {p} → {outPath} (Game ID: {gameId ?? "None"})");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"ERROR: Failed to convert {p} - {ex.Message}");
                        continue;
                    }
                }
                converted.Add(ToWin(outPath));
            }

            if (converted.Count == 0) return;

            // Use ToolRunner to patch via wstrt
            ToolRunner.RunWstrtPatch(
                toolsPathWin: toolsPath,
                mainDolPathWin: mainDolPath,
                gctFilesWin: converted.ToArray(),
                showWindow: debug
            );
        }
    }
}

