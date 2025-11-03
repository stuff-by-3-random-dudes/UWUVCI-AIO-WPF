using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UWUVCI_AIO_WPF.Helpers
{
    public static class IOHelpers
    {
        public static async Task CopyDirectoryAsync(string sourceDir, string destDir, int maxParallel = 4, CancellationToken ct = default)
        {
            if (!Directory.Exists(sourceDir))
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");

            Directory.CreateDirectory(destDir);

            var allFiles = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
            using var limiter = new SemaphoreSlim(maxParallel);
            var tasks = new List<Task>(allFiles.Length);

            foreach (var file in allFiles)
            {
                await limiter.WaitAsync(ct).ConfigureAwait(false);
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        ct.ThrowIfCancellationRequested();
                        string rel = file.Substring(sourceDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        string target = Path.Combine(destDir, rel);
                        Directory.CreateDirectory(Path.GetDirectoryName(target) ?? destDir);
                        await CopyFileBufferedAsync(file, target, ct).ConfigureAwait(false);
                    }
                    finally
                    {
                        limiter.Release();
                    }
                }, ct));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public static void CopyDirectorySync(string sourceDir, string destDir, int maxParallel = 4)
        {
            CopyDirectoryAsync(sourceDir, destDir, maxParallel).GetAwaiter().GetResult();
        }

        public static bool MoveOrCopyDirectory(string sourceDir, string destDir, int maxParallel = 4)
        {
            try
            {
                // If destination exists, remove it so Move can rename atomically
                if (Directory.Exists(destDir)) Directory.Delete(destDir, true);
                Directory.Move(sourceDir, destDir);
                return true;
            }
            catch
            {
                CopyDirectorySync(sourceDir, destDir, maxParallel);
                return false;
            }
        }

        private static async Task CopyFileBufferedAsync(string source, string dest, CancellationToken ct)
        {
            const int buffer = 1024 * 1024; // 1MB
            using var src = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, buffer, useAsync: true);
            using var dst = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None, buffer, useAsync: true);
            await src.CopyToAsync(dst, buffer, ct).ConfigureAwait(false);
        }
    }
}

