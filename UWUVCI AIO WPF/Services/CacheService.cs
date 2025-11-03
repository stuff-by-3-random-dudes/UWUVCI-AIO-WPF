using System;
using System.IO;

namespace UWUVCI_AIO_WPF.Services
{
    public static class CacheService
    {
        public static bool ClearAll()
        {
            bool ok = true;
            try
            {
                // Clear BASE cache
                ok &= BaseExtractor.ClearCache();

                // Clear temporary working directory (bin/temp)
                try
                {
                    string tempPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "temp");
                    if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
                }
                catch { ok = false; }
            }
            catch { ok = false; }
            return ok;
        }
    }
}

