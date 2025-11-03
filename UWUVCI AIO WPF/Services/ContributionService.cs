using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UWUVCI_AIO_WPF.Services
{
    public static class ContributionService
    {
        public static async Task<string> SubmitImagesAndIniAsync(
            string owner,
            string repo,
            string consoleKey,
            string gameId,
            string gameName,
            string[] imagePathsOrNull,
            string iniPathOrNull,
            bool uploadOnlyIfMissing = true)
        {
            var appVersion = FileVersionInfo
                .GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location)
                .FileVersion ?? "unknown";

            var svc = new GitHubImageService();
            return await svc.SubmitImagesAndIniPrAsync(
                owner,
                repo,
                consoleKey,
                gameId,
                gameName,
                appVersion,
                imagePathsOrNull,
                iniPathOrNull,
                new[] { "iconTex", "bootTvTex", "bootDrcTex", "bootLogoTex" },
                uploadOnlyIfMissing
            );
        }
    }
}

