using CNUSPACKER.Models;
using GameBaseClassLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using UWUVCI_AIO_WPF.Classes;
using UWUVCI_AIO_WPF.Helpers;
using UWUVCI_AIO_WPF.Models;
using UWUVCI_AIO_WPF.UI.Windows;
using UWUVCI_AIO_WPF.Services;
using WiiUDownloaderLibrary;
using WiiUDownloaderLibrary.Models;

namespace UWUVCI_AIO_WPF
{

    public static class StringExtensions
    {
        public static string ToHex(this string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
                sb.AppendFormat("{0:X2}", (int)c);
            return sb.ToString().Trim();
        }
    }
    internal static class Injection
    {
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);
        [DllImport("user32.dll")]
        public static extern int SendMessage(
            int hWnd,     // handle to destination window
            uint Msg,      // message
            long wParam,   // first message parameter
            long lParam    // second message parameter
        );
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, int Msg, System.Windows.Forms.Keys wParam, int lParam);
        private static int WM_KEYUP = 0x101;
        private static readonly string tempPath = PathResolver.GetTempPath();
        private static readonly string baseRomPath = Path.Combine(PathResolver.GetTempPath(), "baserom");
        private static readonly string imgPath = Path.Combine(tempPath, "img");
        private static readonly string toolsPath = PathResolver.GetToolsPath();
        static string code = null;
        static MainViewModel mvvm;
        private static bool IsNativeWindows = !MacLinuxHelper.EnvDetect.Get().UnderWineLike;

        /*
         * GameConsole: Can either be NDS, N64, GBA, NES, SNES or TG16
         * baseRom = Name of the BaseRom, which is the folder name too (example: Super Metroid EU will be saved at the BaseRom path under the folder SMetroidEU, so the BaseRom is in this case SMetroidEU).
         * customBasePath = Path to the custom Base. Is null if no custom base is used.
         * injectRomPath = Path to the Rom to be injected into the Base Game.
         * bootImages = String array containing the paths for
         *              bootTvTex: PNG or TGA (PNG gets converted to TGA using UPNG). Needs to be in the dimensions 1280x720 and have a bit depth of 24. If null, the original BootImage will be used.
         *              bootDrcTex: PNG or TGA (PNG gets converted to TGA using UPNG). Needs to be in the dimensions 854x480 and have a bit depth of 24. If null, the original BootImage will be used.
         *              iconTex: PNG or TGA (PNG gets converted to TGA using UPNG). Needs to be in the dimensions 128x128 and have a bit depth of 32. If null, the original BootImage will be used.
         *              bootLogoTex: PNG or TGA (PNG gets converted to TGA using UPNG). Needs to be in the dimensions 170x42 and have a bit depth of 32. If null, the original BootImage will be used.
         * gameName = The name of the final game to be entered into the .xml files.
         * iniPath = Only used for N64. Path to the INI configuration. If "blank", a blank ini will be used.
         * darkRemoval = Only used for N64. Indicates whether the dark filter should be removed.
         */
        static List<int> fiind(this byte[] buffer, byte[] pattern, int startIndex)
        {
            List<int> positions = new List<int>();
            int i = Array.IndexOf(buffer, pattern[0], startIndex);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual(pattern))
                    positions.Add(i);
                i = Array.IndexOf(buffer, pattern[0], i + 1);
            }
            return positions;
        }
        static void PokePatch(string rom)
        {
            byte[] search = { 0xD0, 0x88, 0x8D, 0x83, 0x42 };
            byte[] test;
            test = new byte[new FileInfo(rom).Length];
            using (var fs = new FileStream(rom,
                                 FileMode.Open,
                                 FileAccess.ReadWrite))
            {
                try
                {
                    fs.Read(test, 0, test.Length - 1);

                    var l = fiind(test, search, 0);
                    byte[] check = new byte[4];
                    fs.Seek(l[0] + 5, SeekOrigin.Begin);
                    fs.Read(check, 0, 4);

                    fs.Seek(0, SeekOrigin.Begin);
                    if (check[3] != 0x24)
                    {
                        fs.Seek(l[0] + 5, SeekOrigin.Begin);
                        fs.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0, 4);

                    }
                    else
                    {
                        fs.Seek(l[0] + 5, SeekOrigin.Begin);
                        fs.Write(new byte[] { 0x00, 0x00, 0x00 }, 0, 3);

                    }
                    check = new byte[4];
                    fs.Seek(l[1] + 5, SeekOrigin.Begin);
                    fs.Read(check, 0, 4);
                    fs.Seek(0, SeekOrigin.Begin);
                    if (check[3] != 0x24)
                    {
                        fs.Seek(l[1] + 5, SeekOrigin.Begin);
                        fs.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0, 4);

                    }
                    else
                    {
                        fs.Seek(l[1] + 5, SeekOrigin.Begin);
                        fs.Write(new byte[] { 0x00, 0x00, 0x00 }, 0, 3);

                    }
                }
                catch (Exception)
                {

                }

                fs.Close();
            }
        }
        private static string FormatBytes(long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return string.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }
        [STAThread]
        public static bool Inject(GameConfig Configuration, string RomPath, MainViewModel mvm, bool force)
        {

            mvm.failed = false;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += tick;

            Clean();

            long freeSpaceInBytes = 0;
            if (!mvm.saveworkaround)
            {
                try
                {
                    long gamesize = new FileInfo(RomPath).Length;

                    var drive = new DriveInfo(tempPath);

                    done = true;
                    freeSpaceInBytes = drive.AvailableFreeSpace;
                }
                catch (Exception)
                {
                    mvm.saveworkaround = true;
                }

            }
            mvvm = mvm;

            Directory.CreateDirectory(tempPath);

            mvm.msg = "Checking Tools...";
            mvm.InjcttoolCheck();

            mvm.Progress = 5;

            mvm.msg = "Copying Base...";
            try
            {
                if (!mvm.saveworkaround && (Configuration.Console == GameConsoles.WII || Configuration.Console == GameConsoles.GCN))
                {
                    long neededspace = mvm.GC ? 10000000000 : 15000000000;

                    if (freeSpaceInBytes < neededspace)
                        throw new Exception("Insufficient Storage");
                }

                if (Configuration.BaseRom == null || Configuration.BaseRom.Name == null)
                {
                    throw new Exception("BASE");
                }
                if (Configuration.BaseRom.Name != "Custom")
                {
                    //Normal Base functionality here
                    CopyBase($"{Configuration.BaseRom.Name.Replace(":", "")} [{Configuration.BaseRom.Region}]", null);
                }
                else
                {
                    //Custom Base Functionality here
                    CopyBase($"Custom", Configuration.CBasePath);
                }
                if (!Directory.Exists(Path.Combine(baseRomPath, "code")) || !Directory.Exists(Path.Combine(baseRomPath, "content")) || !Directory.Exists(Path.Combine(baseRomPath, "meta")))
                {
                    throw new Exception("Incomplete Base");
                }
                mvm.Progress = 10;
                mvm.msg = "Injecting ROM...";

                RunSpecificInjection(Configuration, (mvm.GC ? GameConsoles.GCN : Configuration.Console), RomPath, force, mvm);

                mvm.msg = "Editing XML...";
                EditXML(Configuration.GameName, mvm.Index, code, Configuration.GameShortName);
                mvm.Progress = 90;
                mvm.msg = "Changing Images...";
                Images(Configuration);
                if (File.Exists(mvm.BootSound))
                {
                    mvm.Progress = 95;
                    mvm.msg = "Adding BootSound...";
                    bootsound(mvm.BootSound);
                }

                mvm.Progress = 100;

                // --- Unified Community Contribution Prompt (Images + optional INI for N64) ---
                try
                {
                    bool hasCustomImages =
                        mvm.CanOfferImageContribution &&
                        (
                            !string.IsNullOrWhiteSpace(mvm.GameConfiguration.TGADrc?.ImgPath) ||
                            !string.IsNullOrWhiteSpace(mvm.GameConfiguration.TGALog?.ImgPath) ||
                            !string.IsNullOrWhiteSpace(mvm.GameConfiguration.TGAIco?.ImgPath) ||
                            !string.IsNullOrWhiteSpace(mvm.GameConfiguration.TGATv?.ImgPath)
                        );

                    bool isN64 = Configuration.Console == GameConsoles.N64;
                    bool canOfferIni = isN64 && mvm.CanOfferIniContribution;
                    string iniPath = mvm.GameConfiguration?.N64Stuff?.INIPath;
                    bool userProvidedIni = !string.IsNullOrWhiteSpace(iniPath) && File.Exists(iniPath);

                    if (hasCustomImages || (canOfferIni && userProvidedIni))
                    {
                        // Build ordered list (do not filter) to preserve logical key positions
                        var imagePaths = new[]
                        {
                            mvm.GameConfiguration.TGAIco?.ImgPath,
                            mvm.GameConfiguration.TGATv?.ImgPath,
                            mvm.GameConfiguration.TGADrc?.ImgPath,
                            mvm.GameConfiguration.TGALog?.ImgPath
                        };

                        // Normalize each provided path to an existing PNG using the textbox path as the source of truth.
                        // Normalize all paths (exact PNG, then peer directory fallback by canonical keys)
                        string[] canonical = { "iconTex", "bootTvTex", "bootDrcTex", "bootLogoTex" };
                        var normalizedPaths = ImagePathResolver.NormalizeAll(imagePaths, canonical);

                        // If any images are being submitted, require both iconTex and bootTvTex as PNG-resolvable
                        bool anyImagesProvided = normalizedPaths.Any(p => !string.IsNullOrWhiteSpace(p));
                        if (anyImagesProvided)
                        {
                            var icoPng = normalizedPaths[0];
                            var tvPng = normalizedPaths[1];
                            if (icoPng == null || tvPng == null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    UWUVCI_MessageBox.Show(
                                        "Missing Required Images",
                                        "To submit images, both iconTex and bootTvTex must exist as PNGs.\nWe check the paths you selected; if that file doesn’t exist, we try the same path with .png.",
                                        UWUVCI_MessageBoxType.Ok,
                                        UWUVCI_MessageBoxIcon.Warning,
                                        mvm.mw
                                    );
                                });
                                return true; // end injection without starting submission
                            }
                            else
                            {
                                // Reflect resolved PNGs back into the UI so users see what will be submitted
                                if (mvm.Thing is UI.Frames.InjectFrames.Configurations.N64Config n64)
                                {
                                    try
                                    {
                                        n64.Dispatcher.Invoke(() =>
                                        {
                                            if (!string.IsNullOrWhiteSpace(normalizedPaths[0])) n64.ic.Text = normalizedPaths[0];
                                            if (!string.IsNullOrWhiteSpace(normalizedPaths[1])) n64.tv.Text = normalizedPaths[1];
                                            if (!string.IsNullOrWhiteSpace(normalizedPaths[2])) n64.drc.Text = normalizedPaths[2];
                                            if (!string.IsNullOrWhiteSpace(normalizedPaths[3])) n64.log.Text = normalizedPaths[3];
                                        });
                                    }
                                    catch { }
                                }
                            }
                        }

                        string body = hasCustomImages && canOfferIni && userProvidedIni
                            ? "You’ve provided custom images and a custom INI for this inject.\n\nWould you like to share them with the community?\n\nUWUVCI-ContriBot will automatically create a single pull request containing your files."
                            : hasCustomImages
                                ? "You’ve provided custom boot images for this inject.\n\nWould you like to share them with the community?\n\nUWUVCI-ContriBot will automatically create a pull request containing your images."
                                : "No official INI was found for this title.\n\nIf your INI works well, would you like to submit it to help others?\n\nNote: Community INIs are marked as user-submitted.";

                        UWUVCI_MessageBoxResult res = UWUVCI_MessageBoxResult.No;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            res = UWUVCI_MessageBox.Show(
                                "Share Your Files?",
                                body,
                                UWUVCI_MessageBoxType.YesNo,
                                UWUVCI_MessageBoxIcon.Info,
                                mvm.mw
                            );
                        });

                        if (res == UWUVCI_MessageBoxResult.Yes)
                        {
                            string consoleName = Configuration.Console.ToString();
                            string repoId = mvm.repoId;
                            string gameName = Configuration.GameName ?? "Unknown Game";
                            string owner = "UWUVCI-PRIME";
                            string repo = "UWUVCI-IMAGES";

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                UWUVCI_MessageBox.Show(
                                    "Submission Started",
                                    "Your files are being submitted in the background.\nA pull request will be created if successful.",
                                    UWUVCI_MessageBoxType.Ok,
                                    UWUVCI_MessageBoxIcon.Success,
                                    mvm.mw,
                                    isModal: false
                                );
                            });

                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    var prUrl = await ContributionService.SubmitImagesAndIniAsync(
                                        owner,
                                        repo,
                                        consoleName,
                                        repoId,
                                        gameName,
                                        hasCustomImages ? normalizedPaths : null,
                                        (canOfferIni && userProvidedIni) ? iniPath : null,
                                        uploadOnlyIfMissing: true
                                    );

                                    await Application.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        if (string.IsNullOrWhiteSpace(prUrl))
                                        {
                                            UWUVCI_MessageBox.Show(
                                                "Access Restricted",
                                                "Your device is not allowed to submit files.\n\nIf you believe this is an error, please contact support.",
                                                UWUVCI_MessageBoxType.Ok,
                                                UWUVCI_MessageBoxIcon.Error,
                                                mvm.mw
                                            );
                                        }
                                        else
                                        {
                                            UWUVCI_MessageBox.Show(
                                                "Files Submitted",
                                                $"Your files have been submitted successfully!\n\nA Pull Request has been created:\n{prUrl}",
                                                UWUVCI_MessageBoxType.Ok,
                                                UWUVCI_MessageBoxIcon.Success,
                                                mvm.mw,
                                                isModal: false
                                            );
                                        }
                                    });
                                }
                                catch (Exception ex)
                                {
                                    await Application.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        UWUVCI_MessageBox.Show(
                                            "Submission Failed",
                                            $"An error occurred while submitting your files.\n\n{ex.Message}",
                                            UWUVCI_MessageBoxType.Ok,
                                            UWUVCI_MessageBoxIcon.Error,
                                            mvm.mw
                                        );
                                    });
                                }
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error while offering contribution: {ex.Message}");
                }

                code = null;
                return true;
            }
            catch (Exception e)
            {
                mvm.Progress = 100;
                code = null;

                // Handle known internal cleanup triggers
                if (e.Message == "Failed this shit")
                {
                    Clean();
                    return false;
                }

                string errorMessage =
                    "The injection process encountered an unexpected error and could not complete successfully.";

                // --- Specific error handling ---
                if (e.Message == "Incomplete Base")
                {
                    errorMessage =
                        "❌ Missing or incomplete base files were detected.\n" +
                        "Please redownload the base files, or re-dump your base if you created a custom one.";
                }
                else if (e.Message.Contains("Images"))
                {
                    errorMessage =
                        "🖼️ One or more image files are incompatible with the expected bit depth.\n" +
                        "Ensure all images are 24-bit or 32-bit (no 8-bit or indexed formats).\n\n" +
                        "If your images were automatically fetched, replace them with verified ones.";
                }
                else if (e.Message.Contains("Size"))
                {
                    errorMessage =
                        "🖼️ Image dimensions or bit depths appear to be incorrect.\n" +
                        "Please verify your image specifications:\n\n" +
                        "• iconTex → 128×128, 32-bit\n" +
                        "• bootDrcTex → 854×480, 24-bit\n" +
                        "• bootTvTex → 1280×720, 24-bit\n" +
                        "• bootLogoTex → 170×42, 32-bit";
                }
                else if (e.Message.Contains("retro"))
                {
                    errorMessage =
                        "💾 The selected ROM is too large for the chosen base.\n" +
                        "Try again using a different base that supports larger ROMs.";
                }
                else if (e.Message.Contains("BASE"))
                {
                    errorMessage =
                        "⚙️ Configuration imported successfully, but no base was re-selected.\n" +
                        "After importing a config, please re-select your desired base before injecting.";
                }
                else if (e.Message.Contains("WII"))
                {
                    errorMessage =
                        $"💽 {e.Message.Replace("Wii", "")}\n\n" +
                        "Ensure your ROM isn’t corrupted and that you have at least 12 GB of free disk space available.";
                }
                else if (e.Message?.IndexOf("Insufficient Storage", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    errorMessage =
                        $"💾 Not enough storage space available.\n" +
                        $"Ensure at least {FormatBytes(15_000_000_000)} of free space on the drive where UWUVCI is installed.";
                }
                else if (e.Message.Contains("nkit"))
                {
                    errorMessage =
                        "⚠️ The selected NKIT file is invalid or incomplete.\n" +
                        "Use a full, unmodified ISO instead or re-dump your game properly.";
                }
                else if (e.Message.Contains("meta.xml"))
                {
                    errorMessage =
                        "📄 The meta.xml file could not be found in your base directory.\n" +
                        "If using a downloaded base, redownload it.\n" +
                        "If using a custom base, verify your folder layout is correct.";
                }
                else if (e.Message.Contains("pre.iso"))
                {
                    errorMessage =
                        "💿 The game image appears too trimmed.\n" +
                        "Use a full, clean ISO dump instead.";
                }
                else if (e.Message.Contains("temp\\temp") || e.Message.Contains("temp/temp"))
                {
                    errorMessage =
                        "🖼️ An image-related issue occurred during processing.\n" +
                        "Try using different images or re-exporting them in a standard format.";
                }

                // --- Emulation warning ---
                if (!IsNativeWindows)
                {
                    errorMessage +=
                        "\n\n⚠️ UWUVCI detected that you may be running under a compatibility layer (Wine, Proton, etc.).\n" +
                        "Some external tools may not function correctly in non-Windows environments.\n" +
                        "For best results, use native Windows or a verified Wine configuration.";
                }

                // --- Add friendly FAQ reminder ---
                errorMessage +=
                    "\n\n💡 For more help, open the Settings (⚙️) at the top right and check the FAQ section in the ReadMe.";

                // --- Include underlying error details and log location to aid debugging ---
                try
                {
                    if (!string.IsNullOrWhiteSpace(e?.Message))
                    {
                        // Keep details concise but useful
                        string details = e.Message;
                        if (details.Length > 900) details = details.Substring(0, 900) + "…";
                        errorMessage += "\n\nDetails:\n" + details;
                    }

                    string logsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UWUVCI-V3", "Logs");
                    errorMessage += "\n\nLogs:\n" + logsDir;
                }
                catch { /* best-effort */ }

                Logger.Log($"Injection error: {e.Message}");

                // --- Display to user on UI thread ---
                try
                {
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        UWUVCI_MessageBox.Show(
                            "❌ Injection Failed",
                            errorMessage,
                            UWUVCI_MessageBoxType.Ok,
                            UWUVCI_MessageBoxIcon.Error,
                            mvm?.mw
                        );
                    });
                }
                catch
                {
                    // Fallback if dispatcher is unavailable
                    try
                    {
                        UWUVCI_MessageBox.Show(
                            "❌ Injection Failed",
                            errorMessage,
                            UWUVCI_MessageBoxType.Ok,
                            UWUVCI_MessageBoxIcon.Error
                        );
                    }
                    catch { }
                }

                Clean();
                return false;
            }
            finally
            {
                mvm.Index = -1;
                mvm.LR = false;
                mvm.msg = "";
            }

        }

        private static bool done = false;
        private static void tick(object sender, EventArgs e)
        {
            if (!done)
                mvvm.failed = true;

            throw new Exception("Failed this shit");
        }

        public static void SendKey(IntPtr hWnd, System.Windows.Forms.Keys key)
        {
            PostMessage(hWnd, WM_KEYUP, key, 0);
        }
        static void bootsound(string sound)
        {
            string btsndPath = Path.Combine(baseRomPath, "meta", "bootSound.btsnd");
            FileInfo soundFile = new FileInfo(sound);
            if (soundFile.Extension.Contains("mp3") || soundFile.Extension.Contains("wav"))
            {
                // Convert input file to 6 second .wav
                using (Process sox = new Process())
                {
                    sox.StartInfo.UseShellExecute = false;
                    sox.StartInfo.CreateNoWindow = true;
                    sox.StartInfo.FileName = Path.Combine(toolsPath, "sox.exe");
                    sox.StartInfo.Arguments = $"\"{sound}\" -b 16 \"{Path.Combine(tempPath, "bootSound.wav")}\" channels 2 rate 48k trim 0 6";
                    sox.Start();
                    sox.WaitForExit();
                }
                //convert to btsnd
                wav2btsnd(Path.Combine(tempPath, "bootSound.wav"), btsndPath);
                File.Delete(Path.Combine(tempPath, "bootSound.wav"));
            }
            else
            {
                //Copy BootSound to location
                File.Delete(btsndPath);
                File.Copy(sound, btsndPath);
            }
        }

        private static void wav2btsnd(string inputWav, string outputBtsnd)
        {
            // credits to the original creator of wav2btsnd for the general logic
            byte[] buffer = File.ReadAllBytes(inputWav);
            using FileStream output = new FileStream(outputBtsnd, FileMode.OpenOrCreate);
            using BinaryWriter writer = new BinaryWriter(output);

            writer.Write(new byte[] { 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x0 });
            for (int i = 0x2C; i < buffer.Length; i += 2)
                writer.Write(new[] { buffer[i + 1], buffer[i] });
        }

        static void timer_Tick(object sender, EventArgs e)
        {
            if (mvvm.Progress < 50)
                mvvm.Progress += 1;

        }
        private static void RunSpecificInjection(GameConfig cfg, GameConsoles console, string RomPath, bool force, MainViewModel mvm)
        {
            switch (console)
            {
                case GameConsoles.NDS:
                    NDS(RomPath);
                    break;

                case GameConsoles.N64:
                    N64(RomPath, cfg.N64Stuff);
                    break;

                case GameConsoles.GBA:
                    GBA(RomPath, cfg.GBAStuff);
                    break;

                case GameConsoles.NES:
                    NESSNES(RomPath);
                    break;
                case GameConsoles.SNES:
                    NESSNES(RemoveHeader(RomPath));
                    break;
                case GameConsoles.TG16:
                    TG16(RomPath);
                    break;
                case GameConsoles.MSX:
                    MSX(RomPath);
                    break;
                case GameConsoles.WII:
                    WII_RunOrchestrated(RomPath, mvm);
                    break;
                case GameConsoles.GCN:
                    GCN(RomPath, mvm, force);
                    break;
            }
        }
        private static void WII_RunOrchestrated(string romPath, MainViewModel mvm)
        {
            void Log(string msg)
            {
                var line = $"[WII] {DateTime.Now:HH:mm:ss} {msg}";
                Console.WriteLine(line);
                try { Logger.Log(line); } catch { }
            }
            Stopwatch StepTimer(string name, int step)
            {
                Log($"STEP {step}: {name} — START");
                return Stopwatch.StartNew();
            }
            void EndTimer(Stopwatch sw, int step) => Log($"STEP {step}: done in {sw.Elapsed.TotalSeconds:F2}s");

            romPath = ToolRunner.ToWindowsView(romPath);
            var tempBase = string.Empty;

            // Patch callback only when GCT paths provided
            Func<string, bool> patchCb = null;
            if (!string.IsNullOrWhiteSpace(mvm.gctPath) || mvm.RemoveDeflicker || mvm.RemoveDithering || mvm.HalfVFilter || mvm.Index == 4)
            {
                var gcts = string.IsNullOrWhiteSpace(mvm.gctPath)
                    ? Array.Empty<string>()
                    : mvm.gctPath.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

                patchCb = (dolPath) =>
                {
                    try
                    {
                        // Apply GCT patches first (if any)
                        if (gcts.Length > 0)
                        {
                            GctPatcherService.PatchWiiDolWithGcts(toolsPath, dolPath, gcts, mvm.debug);
                            ToolRunner.WaitForWineVisibility(dolPath);
                            ToolRunner.WaitForStableFileSize(dolPath, delayMs: JsonSettingsManager.Settings.UnixWaitDelayMs);
                        }

                        // Deflicker / dithering / half v-filter
                        if (mvm.RemoveDeflicker || mvm.RemoveDithering || mvm.HalfVFilter)
                        {
                            var output = Path.Combine(Path.GetDirectoryName(dolPath) ?? string.Empty, "patched.dol");
                            DeflickerDitheringRemover.ProcessFile(dolPath, output, mvm.RemoveDeflicker, mvm.RemoveDithering, mvm.HalfVFilter);
                            File.Delete(dolPath);
                            File.Move(output, dolPath);
                            ToolRunner.WaitForWineVisibility(dolPath);
                            ToolRunner.WaitForStableFileSize(dolPath, delayMs: JsonSettingsManager.Settings.UnixWaitDelayMs);
                        }

                        // Force Classic Controller when index == 4
                        if (mvm.Index == 4)
                        {
                            using var tik = new Process();
                            tik.StartInfo.FileName = Path.Combine(toolsPath, "GetExtTypePatcher.exe");
                            tik.StartInfo.Arguments = $"\"{dolPath}\" -nc";
                            tik.StartInfo.UseShellExecute = false;
                            tik.StartInfo.CreateNoWindow = true;
                            tik.StartInfo.RedirectStandardInput = true;
                            tik.Start();
                            Thread.Sleep(2000);
                            tik.StandardInput.WriteLine();
                            tik.WaitForExit();
                            ToolRunner.WaitForWineVisibility(dolPath);
                            ToolRunner.WaitForStableFileSize(dolPath, delayMs: JsonSettingsManager.Settings.UnixWaitDelayMs);
                        }

                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                };
            }

            var opt = new WiiInjectOptions
            {
                Debug = true,
                DontTrim = mvm.donttrim,
                PatchVideo = mvm.Patch,
                ToPal = mvm.toPal,
                Index = mvm.Index,
                LR = mvm.LR,
                ForceNkitConvert = mvm.NKITFLAG,
                Passthrough = mvm.passtrough,
                PatchDolCallback = patchCb,
                Progress = (p, msg) => { if (mvm != null) { try { mvm.Progress = (short)p; mvm.msg = msg; } catch { } } }
            };

            if (romPath.ToLower().EndsWith(".dol"))
            {
                var t = StepTimer("Homebrew Flow", 1);
                if (mvm != null) mvm.msg = "Extracting Homebrew Base...";
                tempBase = WiiInjectService.PrepareTempBase(toolsPath, tempPath);
                WiiInjectService.CopyDolToBase(tempBase, romPath);
                var nfs = new NfsInjectOptions { Debug = opt.Debug, Kind = InjectKind.WiiHomebrew, Passthrough = opt.Passthrough, Index = opt.Index, LR = opt.LR, Progress = opt.Progress };
                WitNfsService.BuildIsoExtractTicketsAndInject(toolsPath, tempPath, baseRomPath, nfs);
                EndTimer(t, 1);
                return;
            }
            if (romPath.ToLower().EndsWith(".wad"))
            {
                var t = StepTimer("Forwarder Flow", 1);
                if (mvm != null) mvm.msg = "Extracting Forwarder Base...";
                tempBase = WiiInjectService.PrepareTempBase(toolsPath, tempPath);
                WiiInjectService.SetupForwarderTitle(tempBase, romPath);
                WiiInjectService.CopyForwarderDol(toolsPath, tempBase);
                var nfs = new NfsInjectOptions { Debug = opt.Debug, Kind = InjectKind.WiiForwarder, Passthrough = opt.Passthrough, Index = opt.Index, LR = opt.LR, Progress = opt.Progress };
                WitNfsService.BuildIsoExtractTicketsAndInject(toolsPath, tempPath, baseRomPath, nfs);
                EndTimer(t, 1);
                return;
            }

            var runner = DefaultToolRunnerFacade.Instance;

            // STEP 1: Prepare pre.iso (or reuse source if already ISO)
            var st1 = StepTimer("Prepare pre.iso", 1);
            var pre = WiiInjectService.PreparePreIso(toolsPath, tempPath, romPath, opt, runner);
            EndTimer(st1, 1);

            if (mvm.regionfrii)
            {
                ApplyRegionFriiPatch(pre.preIso, mvm);
            }

            if (mvm != null) 
            { 
                mvm.Progress = 20; 
                mvm.msg = "Prepared pre.iso"; 
            }

            // STEP 2: Update meta flag from ISO header
            var st2 = StepTimer("Update meta.xml reserved flag", 2);
            WiiInjectService.UpdateMetaReservedFlag(baseRomPath, pre.preIso);
            EndTimer(st2, 2);
            if (mvm != null) { mvm.Progress = 25; mvm.msg = "Updated meta.xml"; }

            // STEP 3: Extract pre.iso to TEMP directory
            var st3 = StepTimer("Extract pre.iso", 3);
            tempBase = Path.Combine(tempPath, "TempBase");
            if (Directory.Exists(tempBase)) Directory.Delete(tempBase, true);
            WiiInjectService.WitExtractToTemp(toolsPath, pre.preIso, tempBase, opt, runner);
            EndTimer(st3, 3);
            if (mvm != null) { mvm.Progress = 30; mvm.msg = "Extracted pre.iso"; }

            // STEP 4: Apply GCT patch if provided
            var st4 = StepTimer("Apply GCT patch", 4);
            WiiInjectService.ApplyOptionalDolPatch(tempBase, opt);
            EndTimer(st4, 4);
            if (mvm != null) { mvm.Progress = 35; mvm.msg = "Patched DOL (if any)"; }

            // STEP 5: Optional video patch
            if (opt.PatchVideo)
            {
                var st5 = StepTimer("Apply video patch", 5);
                WiiInjectService.ApplyVideoPatch(toolsPath, tempBase, opt);
                EndTimer(st5, 5);
                if (mvm != null) { mvm.Progress = 40; mvm.msg = "Applied video patch"; }
            }

            // STEP 6-8: Build ISO, extract tickets, and inject via shared flow
            var st6 = StepTimer("Build and inject content", 6);
            var nfsOptions = new NfsInjectOptions
            {
                Debug = opt.Debug,
                Kind = InjectKind.WiiStandard,
                Passthrough = opt.Passthrough,
                Index = opt.Index,
                LR = opt.LR,
                Progress = opt.Progress
            };

            WitNfsService.BuildIsoExtractTicketsAndInject(toolsPath, tempPath, baseRomPath, nfsOptions, runner);
            EndTimer(st6, 6);

            if (mvm != null) { mvm.Progress = 80; mvm.msg = "Injection complete"; }
            return;
        }

        private static void ApplyRegionFriiPatch(string isoPath, MainViewModel mvm)
        {
            if (string.IsNullOrWhiteSpace(isoPath) || !File.Exists(isoPath)) return;

            try
            {
                using var fs = new FileStream(isoPath, FileMode.Open, FileAccess.ReadWrite);
                fs.Seek(0x4E003, SeekOrigin.Begin);
                if (mvm.regionfriius)
                {
                    fs.WriteByte(0x01);
                    fs.Seek(0x4E010, SeekOrigin.Begin);
                    fs.Write(new byte[] { 0x80, 0x06, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 }, 0, 16);
                }
                else if (mvm.regionfriijp)
                {
                    fs.WriteByte(0x00);
                    fs.Seek(0x4E010, SeekOrigin.Begin);
                    fs.Write(new byte[] { 0x00, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 }, 0, 16);
                }
                else
                {
                    fs.WriteByte(0x02);
                    fs.Seek(0x4E010, SeekOrigin.Begin);
                    fs.Write(new byte[] { 0x80, 0x80, 0x80, 0x00, 0x03, 0x03, 0x04, 0x03, 0x00, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 }, 0, 16);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"RegionFrii patch failed: {ex.Message}");
            }
        }
        internal static void PatchDol(string consoleName, string mainDolPath, MainViewModel mvm)
        {
            // --- Normalize inputs that might arrive as POSIX to Windows-view once ---
            static string ToWin(string p)
            {
                if (string.IsNullOrEmpty(p)) 
                    return p ?? "";

                if (p.Length > 2 && char.IsLetter(p[0]) && p[1] == ':' && (p[2] == '\\' || p[2] == '/'))
                    return p.Replace('/', '\\');                   // already Windows

                if (p[0] == '/')                                   // POSIX -> Z:\...
                    return @"Z:\" + p.TrimStart('/').Replace('/', '\\');

                return p.Replace('/', '\\');
            }
            mainDolPath = ToWin(mainDolPath);

            // Parse/convert .txt → .gct if needed
            var filePaths = mvm.gctPath.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var convertedGctFiles = new List<string>();

            foreach (var path in filePaths)
            {
                string convertedPath = path;

                if (Path.GetExtension(path).Equals(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var (codes, gameId) = GctCode.ParseOcarinaOrDolphinTxtFile(path);
                        convertedPath = Path.ChangeExtension(path, ".gct");
                        GctCode.WriteGctFile(convertedPath, codes, gameId);
                        Logger.Log($"Converted {path} → {convertedPath} (Game ID: {gameId ?? "None"})");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"ERROR: Failed to convert {path} - {ex.Message}");
                        continue;
                    }
                }

                convertedGctFiles.Add(ToWin(convertedPath)); // normalize for ToolRunner
            }

            if (convertedGctFiles.Count == 0)
            {
                Logger.Log("ERROR: No valid GCT files available for patching.");
                return;
            }

            if (consoleName == "Wii")
            {
                // Use ToolRunner: will run wstrt.exe on native Windows,
                // or wstrt-mac / wstrt-linux natively under Wine (with path conversion, chmod +x, quarantine clear)
                ToolRunner.RunWstrtPatch(
                    toolsPathWin: toolsPath,
                    mainDolPathWin: mainDolPath,
                    gctFilesWin: convertedGctFiles.ToArray(),
                    showWindow: mvm.debug
                );
            }
            else
            {
                // Non-Wii path stays native
                var dol = new Dol();
                var allCodes = new List<GctCode>();
                foreach (var filePath in convertedGctFiles)
                    allCodes.AddRange(GctCode.LoadFromFile(filePath));
                dol.PatchDolFile(mainDolPath, allCodes);
            }
        }

        private static void GctPatch(MainViewModel mvm, string consoleName, string isoPath)
        {
            if (string.IsNullOrEmpty(mvm.GctPath))
                return;

            var extraction = Path.Combine(tempPath, "temp");

            mvm.msg = "Patching main.dol with gct file";
            mvm.Progress = 27;

            File.Delete(isoPath);

            var mainDolPath = Directory.GetFiles(extraction, "main.dol", SearchOption.AllDirectories).FirstOrDefault();

            PatchDol(consoleName, mainDolPath, mvm);
        }
        private static void GCN(string romPath, MainViewModel mvm, bool force)
        {
            var opt = new GcnInjectOptions
            {
                Debug = mvm.debug,
                DontTrim = mvm.donttrim,
                Disc2Path = mvm.gc2rom,
                Force43 = force,
                Passthrough = true
            };

            mvm.Progress = 20;
            mvm.msg = "Preparing TempBase Folder...";

            // Explicit orchestration of GCN steps
            var tmpBase = GCNInjectService.PrepareTempBase(toolsPath, tempPath);

            mvm.Progress = 30;
            mvm.msg = "Applying Nintendont Dol...";

            GCNInjectService.ApplyNintendontDol(toolsPath, tmpBase, opt.Force43);

            mvm.Progress = 40;
            mvm.msg = "Setting up Disc 1...";

            GCNInjectService.PlacePrimaryGame(toolsPath, tmpBase, romPath, opt.DontTrim, opt.Debug, DefaultToolRunnerFacade.Instance);

            mvm.Progress = 50;
            mvm.msg = "Setting up Disk 2...";

            GCNInjectService.PlaceDisc2IfAny(toolsPath, tmpBase, romPath, opt.Disc2Path, opt.DontTrim, opt.Debug, DefaultToolRunnerFacade.Instance);

            mvm.Progress = 60;
            mvm.msg = "Preparing files...";

            var nfs = new NfsInjectOptions { Debug = opt.Debug, Kind = InjectKind.GCN, Passthrough = opt.Passthrough, Index = opt.Index, LR = opt.LR };

            mvm.Progress = 80;
            mvm.msg = "Building ISO...";

            WitNfsService.BuildIsoExtractTicketsAndInject(toolsPath, tempPath, baseRomPath, nfs, DefaultToolRunnerFacade.Instance);
        }

       
        public static void MSX(string injectRomPath)
        {
            mvvm.msg = "Reading Header from Base...";
            byte[] test = new byte[0x580B3];
            using (var fs = new FileStream(Path.Combine(baseRomPath, "content" , "msx", "msx.pkg"),
                                 FileMode.Open,
                                 FileAccess.ReadWrite))
            {


                fs.Read(test, 0, 0x580B3);
                fs.Close();
                File.Delete(Path.Combine(baseRomPath, "content", "msx", "msx.pkg"));
            }
            mvvm.Progress = 20;
            mvvm.msg = "Creating new PKG with read Header...";
            using (var fs = new FileStream(Path.Combine(baseRomPath, "content", "msx", "msx.pkg"),
                                 FileMode.OpenOrCreate,
                                 FileAccess.ReadWrite))
            {


                fs.Write(test, 0, 0x580B3);
                fs.Close();

            }
            mvvm.Progress = 30;
            mvvm.msg = "Reading ROM content...";
            using (var fs = new FileStream(injectRomPath,
                                 FileMode.OpenOrCreate,
                                 FileAccess.ReadWrite))
            {


                test = new byte[fs.Length];
                fs.Read(test, 0, test.Length - 1);

            }
            mvvm.Progress = 50;
            mvvm.msg = "Injecting ROM into new PKG...";
            using (var fs = new FileStream(Path.Combine(baseRomPath, "content", "msx", "msx.pkg"),
                                FileMode.Append))
            {

                fs.Write(test, 0, test.Length);

            }
            mvvm.Progress = 80;
        }
        public static void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
                DeleteDirectory(directory);

            try
            {
                Thread.Sleep(0);
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }
        public static void Clean()
        {
            if (Directory.Exists(tempPath))
                DeleteDirectory(tempPath);
        }
        [STAThread]
        public static void Loadiine(string gameName, string gameConsole)
        {
            if (gameName == null || gameName == string.Empty) gameName = "NoName";
            gameName = gameName.Replace("|", " ");
            Regex reg = new Regex("[^a-zA-Z0-9 é -]");
            //string outputPath = Path.Combine(JsonSettingsManager.Settings.InjectionPath, gameName);
            string outputPath = Path.Combine(JsonSettingsManager.Settings.OutPath, $"[LOADIINE][{gameConsole}] {reg.Replace(gameName,"")} [{mvvm.prodcode}]");
            mvvm.foldername = $"[LOADIINE][{gameConsole}] {reg.Replace(gameName, "")} [{mvvm.prodcode}]";
            int i = 0;
            while (Directory.Exists(outputPath))
            {
                outputPath = Path.Combine(JsonSettingsManager.Settings.OutPath, $"[LOADIINE][{gameConsole}] {reg.Replace(gameName, "")} [{mvvm.prodcode}]_{i}");
                mvvm.foldername = $"[LOADIINE][{gameConsole}] {reg.Replace(gameName, "")} [{mvvm.prodcode}]_{i}";
                i++;
            }
            
            DirectoryCopy(baseRomPath,outputPath, true);

            Custom_Message cm = new Custom_Message("Injection Complete", $"To Open the Location of the Inject press Open Folder.\nIf you want the inject to be put on your SD now, press Copy to SD.", JsonSettingsManager.Settings.OutPath);
            try
            {
                cm.Owner = mvvm.mw;
            }catch(Exception )
            {

            }
            cm.ShowDialog();
            Clean();
        }

        public static async Task PackingAsync(string gameName, string gameConsole, MainViewModel mvm)
        {
            // --- step 0: preflight/tool check ---
            mvm.msg = "Checking tools...";
            mvm.InjcttoolCheck();
            mvm.Progress = 20;

            // --- step 1: sanitize name + decide output folder ---
            mvm.msg = "Creating output folder...";

            static string Sanitize(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return "NoName";
                s = s.Replace("|", " ");
                // allow letters, numbers, space, dash; drop everything else
                s = Regex.Replace(s, "[^a-zA-Z0-9 \\-]", "");
                // collapse runs of spaces/dashes
                s = Regex.Replace(s, "[\\s\\-]+", " ").Trim();
                return s.Length == 0 ? "NoName" : s;
            }

            var safeGameName = Sanitize(gameName ?? "NoName");
            var outRoot = JsonSettingsManager.Settings.OutPath; // absolute, user-chosen
            var basePath = baseRomPath;                         // absolute path to baserom (your global)
            var folderBase = $"[WUP][{gameConsole}] {safeGameName}";
            string outputPath = Path.Combine(outRoot, folderBase);

            // de-dup if it already exists
            int suffix = 0;
            while (Directory.Exists(outputPath))
            {
                suffix++;
                outputPath = Path.Combine(outRoot, $"{folderBase}_{suffix}");
            }

            mvm.foldername = Path.GetFileName(outputPath);

            mvm.Progress = 40;
            mvm.msg = "Packing...";

            // --- step 2: clear CNUSPACKER cache safely (best-effort) ---
            try
            {
                var local = Environment.GetEnvironmentVariable("LocalAppData");
                if (!string.IsNullOrEmpty(local))
                {
                    var cache = Path.Combine(local, @"temp\.net\CNUSPACKER");
                    if (Directory.Exists(cache))
                        Directory.Delete(cache, true);
                }
            }
            catch
            {
                // non-fatal
            }

            // --- step 3: ensure dirs + pin CWD to outRoot (avoids stray Downloads/Release) ---
            var oldCwd = Environment.CurrentDirectory;
            try
            {
                Directory.CreateDirectory(outRoot);
                Directory.CreateDirectory(outputPath);
                Environment.CurrentDirectory = outRoot;

                // Build options for CNUSPACKER (replaces old CLI args)
                var options = new CnusPackagerOptions
                {
                    InputPath = basePath,
                    OutputPath = outputPath,
                    EncryptKeyWith = JsonSettingsManager.Settings.Ckey,
                    SkipXmlParsing = false
                };

                try
                {
                    Logger.Log($"[CNUSPACKER] Starting packaging process for {basePath}");

                    // Run CNUSPACKER directly via its library entry
                    var runner = new CNUSPACKER.CnusPackagerRunner(null, null);
                    await runner.RunAsync(options).ConfigureAwait(false);

                    Logger.Log("[CNUSPACKER] Packaging completed successfully.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"[CNUSPACKER] ERROR: Packaging failed for {basePath}");
                    Logger.Log($"[CNUSPACKER] Exception: {ex}");
                    throw new Exception("CNUSPACKER failed. See logs for details.", ex);
                }
            }
            finally
            {
                Environment.CurrentDirectory = oldCwd;
            }

            // --- step 4: wrap-up/cleanup ---
            mvm.Progress = 90;
            mvm.msg = "Cleaning...";

            try
            {
                Clean();
            }
            catch (Exception ex)
            {
                try { Logger.Log("Clean() failed: " + ex.Message); } catch { }
            }

            mvm.Progress = 100;
            mvm.msg = "";
        }


        public static async Task DownloadAsync(MainViewModel mvm, CancellationToken ct)
        {
            // set up working dirs
            var curdir = Directory.GetCurrentDirectory();
            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);

            Directory.CreateDirectory(tempPath);

            try
            {
                mvm.InjcttoolCheck();
                GameBases b = mvm.getBasefromName(mvm.SelectedBaseAsString);
                TKeys key = mvm.getTkey(b);

                var downloader = new Downloader(null, null);

                // Progress adapter: scale download progress (0–100%) into 20–80%
                var progress = new Progress<(double percent, string message)>(update =>
                {
                    // scale into 20–80% range
                    double scaled = 20 + (update.percent * 0.6);
                    if (scaled > 80) scaled = 80;

                    mvm.Progress = Convert.ToInt16(scaled);
                    mvm.msg = update.message;
                });

                // --- Download phase ---
                var result = await downloader.DownloadAsync(
                    new TitleData(b.Tid, key.Tkey),
                    Path.Combine(tempPath, "download"),
                    progress,
                    ct).ConfigureAwait(false);

                if (!result.Success)
                {
                    mvm.msg = "Download failed: " + result.Error;
                    return;
                }

                // --- Decrypt phase (80–100%) ---
                mvm.msg = "Decrypting content...";
                mvm.Progress = 85; // step into decrypt range

                CSharpDecrypt.CSharpDecrypt.Decrypt(new string[]
                {
                    JsonSettingsManager.Settings.Ckey,
                    Path.Combine(tempPath, "download", b.Tid),
                    Path.Combine(JsonSettingsManager.Settings.BasePath,
                    $"{b.Name.Replace(":", "")} [{b.Region}]")
                });

                mvm.Progress = 95;
                mvm.msg = "Cleaning up...";

                // cleanup step
                foreach (string sFile in Directory.GetFiles(
                             Path.Combine(JsonSettingsManager.Settings.BasePath,
                                 $"{b.Name.Replace(":", "")} [{b.Region}]", "content"),
                             "*.nfs"))
                {
                    File.Delete(sFile);
                }

                // only now hit 100 → window will auto-close
                mvm.Progress = 100;
                mvm.msg = "Done.";
            }
            finally
            {
                Directory.SetCurrentDirectory(curdir);
            }
        }


        public static string ExtractBase(string path, GameConsoles console)
        {
            if(!Directory.Exists(Path.Combine(JsonSettingsManager.Settings.BasePath, "CustomBases")))
                Directory.CreateDirectory(Path.Combine(JsonSettingsManager.Settings.BasePath, "CustomBases"));

            string outputPath = Path.Combine(JsonSettingsManager.Settings.BasePath, "CustomBases", $"[{console}] Custom");
            int i = 0;
            while (Directory.Exists(outputPath))
            {
                outputPath = Path.Combine(JsonSettingsManager.Settings.BasePath, $"[{console}] Custom_{i}");
                i++;
            }
            CSharpDecrypt.CSharpDecrypt.Decrypt(new string[] { JsonSettingsManager.Settings.Ckey, path, outputPath });
            return outputPath;
        }
        // This function changes TitleID, ProductCode and GameName in app.xml (ID) and meta.xml (ID, ProductCode, Name)
        private static void EditXML(string gameNameOr, int index, string code, string shortName = "")
        {
            string gameName = string.Empty;
            if(gameNameOr != null || !string.IsNullOrWhiteSpace(gameNameOr))
            {
                gameName = gameNameOr;
                if (gameName.Contains('|'))
                {
                    var split = gameName.Split('|');
                    gameName = split[0] + "," + split[1];
                }
            }

            if (string.IsNullOrEmpty(shortName))
                shortName = gameName.Split(',')[0];

            string metaXml = Path.Combine(baseRomPath, "meta", "meta.xml");
            string appXml = Path.Combine(baseRomPath, "code", "app.xml");
            Random random = new Random();
            string ID = $"{random.Next(0x3000, 0x10000):X4}{random.Next(0x3000, 0x10000):X4}";

            string ID2 = $"{random.Next(0x3000, 0x10000):X4}";
            mvvm.prodcode = ID2;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(metaXml);
                if (gameName != null && gameName != string.Empty)
                {
                    doc.SelectSingleNode("menu/longname_ja").InnerText = gameName.Replace(",", "\n" );
                    doc.SelectSingleNode("menu/longname_en").InnerText = gameName.Replace(",", "\n");
                    doc.SelectSingleNode("menu/longname_fr").InnerText = gameName.Replace(",", "\n");
                    doc.SelectSingleNode("menu/longname_de").InnerText = gameName.Replace(",", "\n");
                    doc.SelectSingleNode("menu/longname_it").InnerText = gameName.Replace(",", "\n");
                    doc.SelectSingleNode("menu/longname_es").InnerText = gameName.Replace(",", "\n");
                    doc.SelectSingleNode("menu/longname_zhs").InnerText = gameName.Replace(",", "\n");
                    doc.SelectSingleNode("menu/longname_ko").InnerText = gameName.Replace(",", "\n");
                    doc.SelectSingleNode("menu/longname_nl").InnerText = gameName.Replace(",", "\n");
                    doc.SelectSingleNode("menu/longname_pt").InnerText = gameName.Replace(",", "\n");
                    doc.SelectSingleNode("menu/longname_ru").InnerText = gameName.Replace(",", "\n");
                    doc.SelectSingleNode("menu/longname_zht").InnerText = gameName.Replace(",", "\n");
                }

                /* if(code != null)
                {
                    doc.SelectSingleNode("menu/product_code").InnerText = $"WUP-N-{code}";
                }
                else
                {*/
                    doc.SelectSingleNode("menu/product_code").InnerText = $"WUP-N-{ID2}";
                //}
                if (index > 0)
                {
                    doc.SelectSingleNode("menu/drc_use").InnerText = "65537";
                }
                doc.SelectSingleNode("menu/title_id").InnerText = $"00050002{ID}";
                doc.SelectSingleNode("menu/group_id").InnerText = $"0000{ID2}";
                if (gameName != null && gameName != string.Empty)
                {
                    doc.SelectSingleNode("menu/shortname_ja").InnerText = shortName;
                    doc.SelectSingleNode("menu/shortname_fr").InnerText = shortName;
                    doc.SelectSingleNode("menu/shortname_de").InnerText = shortName;
                    doc.SelectSingleNode("menu/shortname_en").InnerText = shortName;
                    doc.SelectSingleNode("menu/shortname_it").InnerText = shortName;
                    doc.SelectSingleNode("menu/shortname_es").InnerText = shortName;
                    doc.SelectSingleNode("menu/shortname_zhs").InnerText = shortName;
                    doc.SelectSingleNode("menu/shortname_ko").InnerText = shortName;
                    doc.SelectSingleNode("menu/shortname_nl").InnerText = shortName;
                    doc.SelectSingleNode("menu/shortname_pt").InnerText = shortName;
                    doc.SelectSingleNode("menu/shortname_ru").InnerText = shortName;
                    doc.SelectSingleNode("menu/shortname_zht").InnerText = shortName;
                }

                doc.Save(metaXml);
            }
            catch (NullReferenceException)
            {
                   
            }

            try
            {
                doc.Load(appXml);
                doc.SelectSingleNode("app/title_id").InnerText = $"00050002{ID}";
            //doc.SelectSingleNode("app/title_id").InnerText = $"0005000247414645";
                
                doc.SelectSingleNode("app/group_id").InnerText = $"0000{ID2}";
                doc.Save(appXml);
            }
            catch (NullReferenceException)
            {
                  
            }
        }

        //This function copies the custom or normal Base to the working directory
        private static void CopyBase(string baserom, string customPath)
        {
            if (Directory.Exists(baseRomPath)) // sanity check
                Directory.Delete(baseRomPath, true);

            if (baserom == "Custom")
                DirectoryCopy(customPath, baseRomPath, true);
            else
                DirectoryCopy(Path.Combine(JsonSettingsManager.Settings.BasePath, baserom), baseRomPath, true);
        }

        private static void TG16(string injectRomPath)
        {
            //checking if folder
            if (Directory.Exists(injectRomPath))
            {
                DirectoryCopy(injectRomPath, "test", true);
                //TurboGrafCD
                using (Process TurboInject = new Process())
                {
                    mvvm.msg = "Creating TurboCD Pkg...";
                    TurboInject.StartInfo.UseShellExecute = false;
                    TurboInject.StartInfo.CreateNoWindow = true;
                    TurboInject.StartInfo.FileName = Path.Combine(toolsPath, "BuildTurboCDPcePkg.exe");
                    TurboInject.StartInfo.Arguments = $"test";
                    TurboInject.Start();
                    TurboInject.WaitForExit();
                    mvvm.Progress = 70;
                }
                Directory.Delete("test", true);
            }
            else
            {
                //creating pkg file including the TG16 rom
                using Process TurboInject = new Process();
                mvvm.msg = "Creating Turbo16 Pkg...";
                TurboInject.StartInfo.UseShellExecute = false;
                TurboInject.StartInfo.CreateNoWindow = true;
                TurboInject.StartInfo.FileName = Path.Combine(toolsPath, "BuildPcePkg.exe");
                TurboInject.StartInfo.Arguments = $"\"{injectRomPath}\"";
                TurboInject.Start();
                TurboInject.WaitForExit();
                mvvm.Progress = 70;
            }
            mvvm.msg = "Injecting ROM...";
            //replacing tg16 rom
            File.Delete(Path.Combine(baseRomPath, "content", "pceemu", "pce.pkg"));
            File.Copy("pce.pkg", Path.Combine(baseRomPath, "content", "pceemu", "pce.pkg"));
            File.Delete("pce.pkg");
            mvvm.Progress = 80;
        }

        private static void NESSNES(string injectRomPath)
        {
            string rpxFile = Directory.GetFiles(Path.Combine(baseRomPath, "code"), "*.rpx")[0]; //To get the RPX path where the NES/SNES rom needs to be Injected in
            mvvm.msg = "Decompressing RPX...";
            RPXCompOrDecomp(rpxFile, false); //Decompresses the RPX to be able to write the game into it
            mvvm.Progress = 20;
            if (mvvm.pixelperfect)
            {
                using Process retroinject = new Process();
                mvvm.msg = "Applying Pixel Perfect Patches...";
                retroinject.StartInfo.UseShellExecute = false;
                retroinject.StartInfo.CreateNoWindow = true;
                retroinject.StartInfo.RedirectStandardOutput = true;
                retroinject.StartInfo.RedirectStandardError = true;
                retroinject.StartInfo.FileName = Path.Combine(toolsPath, "ChangeAspectRatio.exe");
                retroinject.StartInfo.Arguments = $"\"{rpxFile}\"";

                retroinject.Start();
                retroinject.WaitForExit();
                mvvm.Progress = 30;
            }
            using (Process retroinject = new Process())
            {
                mvvm.msg = "Injecting ROM...";
                retroinject.StartInfo.UseShellExecute = false;
                retroinject.StartInfo.CreateNoWindow = true;
                retroinject.StartInfo.RedirectStandardOutput = true;
                retroinject.StartInfo.RedirectStandardError = true;
                retroinject.StartInfo.FileName = Path.Combine(toolsPath, "retroinject.exe");
                retroinject.StartInfo.Arguments = $"\"{rpxFile}\" \"{injectRomPath}\" \"{rpxFile}\"";

                retroinject.Start();
                retroinject.WaitForExit();
                mvvm.Progress = 70;
                var s = retroinject.StandardOutput.ReadToEnd();
                var e = retroinject.StandardError.ReadToEnd();
                if (e.Contains("is too large") || s.Contains("is too large"))
                {
                    mvvm.Progress = 100;
                    throw new Exception("retro");
                }

            }
            mvvm.msg = "Compressing RPX...";

            if (mvvm.GameConfiguration.Console == GameConsoles.NES)
            {
                var defaultPalette = mvvm.NesPaletteOptions?.FirstOrDefault()?.Name ?? "Default (Base RPX)";
                NesPalettePatcher.Apply(rpxFile, mvvm.SelectedNesPaletteName, defaultPalette);
            }

            RPXCompOrDecomp(rpxFile, true); //Compresses the RPX
            mvvm.Progress = 80;
        }

        private static void GBA(string injectRomPath, N64Conf config)
        {
            // --- small logger helpers ---
            void Log(string msg)
            {
                var line = $"[GBA] {DateTime.Now:HH:mm:ss} {msg}";
                Console.WriteLine(line);
                try { Logger.Log(line); } catch { /* ignore */ }
            }
            string SizeOf(string p) => File.Exists(p) ? new FileInfo(p).Length.ToString("N0") + " bytes" : "missing";

            // Convenience wrapper for MArchiveBatchTool.exe (or native fallback later)
            void RunMArchive(string args, string label, bool longWaitBump = false)
            {
                mvvm.msg = label;
                Log($"MArchiveBatchTool {args}");
                ToolRunner.RunToolWithFallback(
                    toolBaseName: "MArchiveBatchTool",
                    toolsPathWin: toolsPath,
                    argsWindowsPaths: args,
                    showWindow: mvvm.debug
                );
                if (longWaitBump) 
                    mvvm.Progress += 15; 
                else 
                    mvvm.Progress += 5;
            }

            bool deleteTempRom = false;
            string workingRom = injectRomPath;

            try
            {
                // --- If not a .gba, wrap with goomba (GB/GBC -> GBA) ---
                if (!string.Equals(Path.GetExtension(workingRom), ".gba", StringComparison.OrdinalIgnoreCase))
                {
                    mvvm.msg = "Injecting GB/GBC ROM into goomba...";
                    Log($"Input is not .gba -> building goomba menu with {workingRom}");

                    string goombaGbaPath = Path.Combine(toolsPath, "goomba.gba");
                    string goombaMenuPath = Path.Combine(tempPath, "goombamenu.gba");  // temp instead of tools
                    Directory.CreateDirectory(tempPath);

                    // Concatenate goomba.gba + ROM
                    using (var output = new FileStream(goombaMenuPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (var src = new FileStream(goombaGbaPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                            src.CopyTo(output);

                        using (var romSrc = new FileStream(workingRom, FileMode.Open, FileAccess.Read, FileShare.Read))
                            romSrc.CopyTo(output);
                    }
                    Log($"goombamenu.gba -> {goombaMenuPath} ({SizeOf(goombaMenuPath)})");
                    mvvm.Progress = 20;

                    // Pad to 32 MB without loading the whole file in RAM
                    mvvm.msg = "Padding goomba ROM...";
                    string goombaPaddedPath = Path.Combine(tempPath, "goombaPadded.gba");
                    const long targetSize = 32L * 1024 * 1024; // 32 MiB

                    using (var src = new FileStream(goombaMenuPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var dst = new FileStream(goombaPaddedPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        src.CopyTo(dst);
                        if (dst.Length < targetSize)
                        {
                            // Grow file by seeking to target-1 and writing one zero byte.
                            dst.Seek(targetSize - 1, SeekOrigin.Begin);
                            dst.WriteByte(0);
                        }
                    }
                    Log($"goombaPadded.gba -> {goombaPaddedPath} ({SizeOf(goombaPaddedPath)})");

                    workingRom = goombaPaddedPath;
                    deleteTempRom = true;
                    mvvm.Progress = 40;
                }

                // --- Optional PokePatch ---
                if (mvvm.PokePatch)
                {
                    mvvm.msg = "Applying PokePatch";
                    Log("PokePatch requested");

                    Directory.CreateDirectory(tempPath);
                    string localRom = Path.Combine(tempPath, "rom.gba");
                    File.Copy(workingRom, localRom, overwrite: true);
                    Log($"Copied for patch: {localRom} ({SizeOf(localRom)})");

                    PokePatch(localRom);

                    workingRom = localRom;
                    deleteTempRom = true;
                    mvvm.PokePatch = false; // consume the flag
                    mvvm.Progress = 50;
                }

                // --- Inject into alldata.psb.m via psb tool ---
                mvvm.msg = "Injecting ROM...";
                string alldata = Path.Combine(baseRomPath, "content", "alldata.psb.m");
                Log($"psb: \"{alldata}\" <- \"{workingRom}\"");
                ToolRunner.RunToolWithFallback(
                    toolBaseName: "psb",
                    toolsPathWin: toolsPath,
                    argsWindowsPaths: $"\"{alldata}\" \"{workingRom}\" \"{alldata}\"",
                    showWindow: mvvm.debug
                );
                Log($"After psb injection: {alldata} ({SizeOf(alldata)})");
                mvvm.Progress = 50;

                // --- Dark filter handling (when disabled) ---
                if (config.DarkFilter == false)
                {
                    Log("DarkFilter disabled -> patching title_prof to brightness=1");
                    string extractedDir = Path.Combine(baseRomPath, "content", "alldata.psb.m_extracted");
                    string builtDir = Path.Combine(baseRomPath, "content", "alldata");
                    Directory.CreateDirectory(builtDir);

                    // 1) Extract archive (longer)
                    RunMArchive($"archive extract \"{alldata}\" --codec zlib --seed MX8wgGEJ2+M47 --keyLength 80", "Extracting archive...", longWaitBump: true);

                    var rootExtract = new DirectoryInfo(extractedDir);

                    var last = (rootExtract.Exists
                        ? rootExtract.GetDirectories().OrderBy(d => d.LastWriteTimeUtc).LastOrDefault()
                        : null) ?? throw new InvalidOperationException("Extraction folder not found after archive extract.");

                    var titleprofPsbM = Path.Combine(last.FullName, "config", "title_prof.psb.m");
                    Log($"Located title_prof.psb.m: {titleprofPsbM}");

                    // 2) Unpack PSB
                    RunMArchive($"m unpack \"{titleprofPsbM}\" zlib MX8wgGEJ2+M47 80", "Unpacking PSB...");

                    var titleprofPsb = Path.Combine(last.FullName, "config", "title_prof.psb");
                    Log($"Unpacked -> {titleprofPsb} ({SizeOf(titleprofPsb)})");

                    // 3) Deserialize to JSON
                    RunMArchive($"psb deserialize \"{titleprofPsb}\"", "Deserializing PSB...");

                    var titleprofJson = Path.Combine(last.FullName, "config", "title_prof.psb.json");
                    Log($"Deserialized JSON -> {titleprofJson} ({SizeOf(titleprofJson)})");

                    // 4) Modify JSON
                    var tmpJson = Path.Combine(last.FullName, "config", "modified_title_prof.psb.json");
                    using (var sr = File.OpenText(titleprofJson))
                    {
                        var json = sr.ReadToEnd();
                        dynamic jsonObj = JsonConvert.DeserializeObject(json);
                        jsonObj["root"]["m2epi"]["brightness"] = 1; // turn off dark filter
                        json = JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                        File.WriteAllText(tmpJson, json);
                    }
                    File.Delete(titleprofJson);
                    File.Move(tmpJson, titleprofJson);
                    Log("Brightness set to 1 in JSON");

                    // 5) Serialize JSON back to PSB
                    RunMArchive($"psb serialize \"{titleprofJson}\"", "Serializing PSB...");

                    // 6) Pack modified PSB back (re-encrypted)
                    RunMArchive($"m pack \"{titleprofPsb}\" zlib MX8wgGEJ2+M47 80", "Packing PSB...");

                    // 7) Rebuild the archive (longer)
                    RunMArchive($"archive build --codec zlib --seed MX8wgGEJ2+M47 --keyLength 80 \"{extractedDir}\" \"{builtDir}\"", "Rebuilding archive...", longWaitBump: true);

                    // Clean up extraction
                    try
                    {
                        if (Directory.Exists(extractedDir))
                        {
                            Directory.Delete(extractedDir, recursive: true);
                            Log($"Cleaned: {extractedDir}");
                        }

                        var alldataPsb = Path.Combine(baseRomPath, "content", "alldata.psb");
                        if (File.Exists(alldataPsb))
                        {
                            File.Delete(alldataPsb);
                            Log($"Removed: {alldataPsb}");
                        }
                    }
                    catch (Exception clex)
                    {
                        Log($"Cleanup warning: {clex.Message}");
                    }
                }

                mvvm.Progress = 80;
            }
            catch (Exception ex)
            {
                Log($"ERROR: {ex}");
                throw;
            }
            finally
            {
                // temp clean-up
                try
                {
                    if (deleteTempRom && File.Exists(workingRom))
                    {
                        File.Delete(workingRom);
                        Log($"Deleted temp ROM: {workingRom}");
                    }

                    string gm = Path.Combine(tempPath, "goombamenu.gba");
                    if (File.Exists(gm))
                    {
                        File.Delete(gm);
                        Log($"Deleted temp: {gm}");
                    }
                }
                catch (Exception clex)
                {
                    Log($"Temp cleanup warning: {clex.Message}");
                }
            }
        }

        private static void NDS(string injectRomPath)
        {
            // --- small logger helpers ---
            void Log(string msg)
            {
                var line = $"[NDS] {DateTime.Now:HH:mm:ss} {msg}";
                Console.WriteLine(line);
                try { Logger.Log(line); } catch { }
            }
            string SizeOf(string p) => File.Exists(p) ? new FileInfo(p).Length.ToString("N0") + " bytes" : "missing";

            try
            {
                string romName = GetRomNameFromZip();
                Log($"Base ROM name in zip: {romName}");
                mvvm.msg = "Removing BaseRom...";
                ReplaceRomWithInjected(romName, injectRomPath);
                Log($"Injected file placed as working ROM: {romName}");

                if (mvvm.DSLayout)
                {
                    mvvm.msg = "Adding additional DS layout screens...";
                    var dest = Path.Combine(tempPath, "DSLayoutScreens");
                    if (Directory.Exists(dest)) Directory.Delete(dest, true);
                    using (var zip = ZipFile.Open(Path.Combine(toolsPath, "DSLayoutScreens.zip"), ZipArchiveMode.Read))
                        zip.ExtractToDirectory(dest);

                    var folder = Path.Combine(dest, (mvvm.STLayout ? "Phatnom Hourglass" : "All"));
                    Log($"Copying DS layout assets from: {folder}");
                    DirectoryCopy(folder, baseRomPath, true);
                }

                if (mvvm.RendererScale || mvvm.Brightness != 80 || mvvm.PixelArtUpscaler != 0)
                {
                    mvvm.msg = "Updating configuration_cafe.json...";
                    Log("Updating configuration_cafe.json based on UI settings");
                    UpdateConfigurationCafeJson();
                }

                RecompressRom(romName);
                var outZip = Path.Combine(baseRomPath, "content", "0010", "rom.zip");
                Log($"Recompressed zip -> {outZip} ({SizeOf(outZip)})");

                mvvm.Progress = 80;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NDS] ERROR: {ex}");
                throw;
            }
        }


        private static void UpdateConfigurationCafeJson()
        {
            var configurationCafe = Path.Combine(baseRomPath, "content", "0010", "configuration_cafe.json");

            // Load the JSON file
            string jsonContent = File.ReadAllText(configurationCafe);

            // Parse the JSON content
            var jsonObject = JObject.Parse(jsonContent);

            // Update the values
            jsonObject["configuration"]["3DRendering"]["RenderScale"] = (mvvm.RendererScale ? 2 : 1);
            jsonObject["configuration"]["Display"]["Brightness"] = mvvm.Brightness;
            jsonObject["configuration"]["Display"]["PixelArtUpscaler"] = mvvm.PixelArtUpscaler;

            // Write the updated JSON back to the file
            File.WriteAllText(configurationCafe, jsonObject.ToString());
        }

        private static string GetRomNameFromZip()
        {
            mvvm.msg = "Getting BaseRom Name...";
            string zipLocation = Path.Combine(baseRomPath, "content", "0010", "rom.zip");
            string romName = string.Empty;

            using (var zip = ZipFile.Open(zipLocation, ZipArchiveMode.Read))
            {
                var entry = zip.Entries.FirstOrDefault(file => file.Name.Contains("WUP"));
                if (entry != null)
                    romName = entry.Name;
            }
            mvvm.Progress = 15;

            if (string.IsNullOrEmpty(romName))
                throw new InvalidOperationException("ROM name not found in the zip archive.");

            return romName;
        }

        private static void ReplaceRomWithInjected(string romName, string injectRomPath)
        {
            string romPath = Path.Combine(Directory.GetCurrentDirectory(), romName);

            if (File.Exists(romPath))
                File.Delete(romPath);

            string zipLocation = Path.Combine(baseRomPath, "content", "0010", "rom.zip");

            if (File.Exists(zipLocation))
                File.Delete(zipLocation);

            File.Copy(injectRomPath, romPath);
        }

        private static void RecompressRom(string romName)
        {
            string zipLocation = Path.Combine(baseRomPath, "content", "0010", "rom.zip");
            string romPath = Path.Combine(Directory.GetCurrentDirectory(), romName);

            using (var stream = new FileStream(zipLocation, FileMode.Create))
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
                    archive.CreateEntryFromFile(romPath, Path.GetFileName(romPath));

            File.Delete(romPath);
        }

        private static void N64(string injectRomPath, N64Conf config)
        {
            string mainRomPath = Directory.GetFiles(Path.Combine(baseRomPath, "content", "rom"))[0];
            string mainIni = Path.Combine(baseRomPath, "content", "config", $"{Path.GetFileName(mainRomPath)}.ini");
            using (Process n64convert = new Process())
            {
                mvvm.msg = "Injecting ROM...";
                n64convert.StartInfo.UseShellExecute = false;
                n64convert.StartInfo.CreateNoWindow = true;
                n64convert.StartInfo.FileName = Path.Combine(toolsPath, "N64Converter.exe");
                n64convert.StartInfo.Arguments = $"\"{injectRomPath}\" \"{mainRomPath}\"";

                n64convert.Start();
                n64convert.WaitForExit();
                mvvm.Progress = 60;

            }

            if (config.WideScreen || config.DarkFilter)
            {
                using (var fileStream = File.Open(Path.Combine(baseRomPath, "content", "FrameLayout.arc"), FileMode.Open))
                {
                    uint offset = 0;
                    uint size = 0;
                    byte[] offsetB = new byte[4];
                    byte[] sizeB = new byte[4];
                    byte[] nameB = new byte[0x18];
                    var header = new byte[4];

                    byte[] oneOut = BitConverter.GetBytes((float)1);
                    byte[] zeroOut = BitConverter.GetBytes((float)0);

                    byte darkFilter = (byte)(config.DarkFilter ? 0 : 1);
                    byte[] wideScreen = config.WideScreen ? new byte[] { 0x44, 0xF0, 0, 0 } : new byte[] { 0x44, 0xB4, 0, 0 };

                    fileStream.Read(header, 0, 4);

                    if (header[0] == 'S' && header[1] == 'A' && header[2] == 'R' && header[3] == 'C')
                    {
                        fileStream.Position = 0x0C;
                        fileStream.Read(offsetB, 0, 4);

                        offset = (uint)(offsetB[0] << 24 | offsetB[1] << 16 | offsetB[2] << 8 | offsetB[3]);

                        fileStream.Position = 0x38;
                        fileStream.Read(offsetB, 0, 4);
                        offset += (uint)(offsetB[0] << 24 | offsetB[1] << 16 | offsetB[2] << 8 | offsetB[3]);

                        fileStream.Position = offset;
                        fileStream.Read(header, 0, 4);

                        if (header[0] == 'F' && header[1] == 'L' && header[2] == 'Y' && header[3] == 'T')
                        {
                            fileStream.Position = offset + 0x04;
                            fileStream.Read(offsetB, 0, 4);

                            offsetB[0] = 0;
                            offsetB[1] = 0;

                            offset += (uint)(offsetB[0] << 24 | offsetB[1] << 16 | offsetB[2] << 8 | offsetB[3]);

                            fileStream.Position = offset;

                            while (true)
                            {
                                fileStream.Read(header, 0, 4);
                                fileStream.Read(sizeB, 0, 4);
                                size = (uint)(sizeB[0] << 24 | sizeB[1] << 16 | sizeB[2] << 8 | sizeB[3]);

                                if (header[0] == 'p' && header[1] == 'i' && header[2] == 'c' && header[3] == '1')
                                {
                                    fileStream.Position = offset + 0x0C;
                                    fileStream.Read(nameB, 0, 0x18);
                                    int count = Array.IndexOf(nameB, (byte)0);
                                    string name = Encoding.ASCII.GetString(nameB, 0, count);

                                    if (name == "frame")
                                    {
                                        fileStream.Position = offset + 0x2C;
                                        fileStream.WriteByte(zeroOut[3]);
                                        fileStream.WriteByte(zeroOut[2]);
                                        fileStream.WriteByte(zeroOut[1]);
                                        fileStream.WriteByte(zeroOut[0]);

                                        fileStream.Position = offset + 0x30;//TranslationX
                                        fileStream.WriteByte(zeroOut[3]);
                                        fileStream.WriteByte(zeroOut[2]);
                                        fileStream.WriteByte(zeroOut[1]);
                                        fileStream.WriteByte(zeroOut[0]);

                                        fileStream.Position = offset + 0x44;//ScaleX
                                        fileStream.WriteByte(oneOut[3]);
                                        fileStream.WriteByte(oneOut[2]);
                                        fileStream.WriteByte(oneOut[1]);
                                        fileStream.WriteByte(oneOut[0]);

                                        fileStream.Position = offset + 0x48;//ScaleY
                                        fileStream.WriteByte(oneOut[3]);
                                        fileStream.WriteByte(oneOut[2]);
                                        fileStream.WriteByte(oneOut[1]);
                                        fileStream.WriteByte(oneOut[0]);

                                        fileStream.Position = offset + 0x4C;//Widescreen
                                        fileStream.Write(wideScreen, 0, 4);
                                    }
                                    else if (name == "frame_mask")
                                    {
                                        fileStream.Position = offset + 0x08;//Dark filter
                                        fileStream.WriteByte(darkFilter);
                                    }
                                    else if (name == "power_save_bg")
                                    {
                                        //This means we finished frame_mask and frame edits so we can end the loop
                                        break;
                                    }

                                    offset += size;
                                    fileStream.Position = offset;
                                }
                                else if (offset + size >= fileStream.Length)
                                {
                                    //do nothing
                                }
                                else
                                {
                                    offset += size;
                                    fileStream.Position = offset;
                                }
                            }
                        }
                    }
                    fileStream.Close();
                }
                mvvm.Progress = 70;
            }

            mvvm.msg = "Copying INI...";
            if (config.INIBin == null)
            {
                if (config.INIPath == null)
                {
                    File.Delete(mainIni);
                    File.Copy(Path.Combine(toolsPath, "blank.ini"), mainIni);
                }
                else
                {
                    File.Delete(mainIni);
                    File.Copy(config.INIPath, mainIni);
                }
            }
            else
            {
                ReadFileFromBin(config.INIBin, "custom.ini");
                File.Delete(mainIni);
                File.Move("custom.ini", mainIni);
            }
            mvvm.Progress = 80;



        }

        private static void InjectRom(string injectRomPath, Action<string> Log)
        {
            string mainRomPath = Directory.GetFiles(Path.Combine(baseRomPath, "content", "rom"))[0];
            mvvm.msg = "Injecting ROM...";
            Log($"N64Converter: \"{injectRomPath}\" -> \"{mainRomPath}\"");

            ToolRunner.RunToolWithFallback(
                toolBaseName: "N64Converter",
                toolsPathWin: toolsPath,
                argsWindowsPaths: $"\"{injectRomPath}\" \"{mainRomPath}\"",
                showWindow: mvvm.debug);

            mvvm.Progress = 60;
        }

        private static void ApplyCustomSettings(N64Conf config)
        {
            string frameLayoutPath = Path.Combine(baseRomPath, "content", "FrameLayout.arc");

            using (var fileStream = File.Open(frameLayoutPath, FileMode.Open))
            {
                uint offset = 0;
                uint size = 0;
                byte[] offsetB = new byte[4];
                byte[] sizeB = new byte[4];
                byte[] nameB = new byte[0x18];
                var header = new byte[4];

                byte[] oneOut = BitConverter.GetBytes((float)1);
                byte[] zeroOut = BitConverter.GetBytes((float)0);

                byte darkFilter = (byte)(config.DarkFilter ? 0 : 1);
                byte[] wideScreen = config.WideScreen ? new byte[] { 0x44, 0xF0, 0, 0 } : new byte[] { 0x44, 0xB4, 0, 0 };

                fileStream.Read(header, 0, 4);

                if (header.SequenceEqual(new byte[] { (byte)'S', (byte)'A', (byte)'R', (byte)'C' }))
                {
                    fileStream.Position = 0x0C;
                    fileStream.Read(offsetB, 0, 4);
                    offset = (uint)(offsetB[0] << 24 | offsetB[1] << 16 | offsetB[2] << 8 | offsetB[3]);

                    fileStream.Position = 0x38;
                    fileStream.Read(offsetB, 0, 4);
                    offset += (uint)(offsetB[0] << 24 | offsetB[1] << 16 | offsetB[2] << 8 | offsetB[3]);

                    fileStream.Position = offset;
                    fileStream.Read(header, 0, 4);

                    if (header.SequenceEqual(new byte[] { (byte)'F', (byte)'L', (byte)'Y', (byte)'T' }))
                    {
                        fileStream.Position = offset + 0x04;
                        fileStream.Read(offsetB, 0, 4);

                        offsetB[0] = 0;
                        offsetB[1] = 0;

                        offset += (uint)(offsetB[0] << 24 | offsetB[1] << 16 | offsetB[2] << 8 | offsetB[3]);

                        fileStream.Position = offset;

                        while (offset < fileStream.Length)
                        {
                            fileStream.Read(header, 0, 4);
                            fileStream.Read(sizeB, 0, 4);
                            size = (uint)(sizeB[0] << 24 | sizeB[1] << 16 | sizeB[2] << 8 | sizeB[3]);

                            if (header[0] == 'p' && header[1] == 'i' && header[2] == 'c' && header[3] == '1')
                            {
                                fileStream.Position = offset + 0x0C;
                                fileStream.Read(nameB, 0, 0x18);

                                int count = Array.IndexOf(nameB, (byte)0);
                                string name = Encoding.ASCII.GetString(nameB, 0, count);

                                if (name == "frame")
                                    WriteFrameData(fileStream, offset, zeroOut, oneOut, wideScreen);
                                else if (name == "frame_mask")
                                    WriteDarkFilterData(fileStream, offset, darkFilter);
                                else if (name == "power_save_bg")
                                    break; // End the loop as the required modifications are done

                            }

                            offset += size;
                            fileStream.Position = offset;
                        }
                    }
                }
            }
            mvvm.Progress = 70;
        }

        private static void WriteFrameData(FileStream fileStream, uint offset, byte[] zeroOut, byte[] oneOut, byte[] wideScreen)
        {
            fileStream.Position = offset + 0x2C;
            fileStream.Write(zeroOut, 0, zeroOut.Length);

            fileStream.Position = offset + 0x30; // TranslationX
            fileStream.Write(zeroOut, 0, zeroOut.Length);

            fileStream.Position = offset + 0x44; // ScaleX
            fileStream.Write(oneOut, 0, oneOut.Length);

            fileStream.Position = offset + 0x48; // ScaleY
            fileStream.Write(oneOut, 0, oneOut.Length);

            fileStream.Position = offset + 0x4C; // Widescreen
            fileStream.Write(wideScreen, 0, wideScreen.Length);
        }

        private static void WriteDarkFilterData(FileStream fileStream, uint offset, byte darkFilter)
        {
            fileStream.Position = offset + 0x08; // Dark filter
            fileStream.WriteByte(darkFilter);
        }

        private static void ApplyIniSettings(N64Conf config)
        {
            mvvm.msg = "Copying INI...";
            string mainRomPath = Directory.GetFiles(Path.Combine(baseRomPath, "content", "rom"))[0];
            string mainIni = Path.Combine(baseRomPath, "content", "config", $"{Path.GetFileName(mainRomPath)}.ini");

            if (config.INIBin == null)
            {
                if (config.INIPath == null)
                {
                    File.Delete(mainIni);
                    File.Copy(Path.Combine(toolsPath, "blank.ini"), mainIni);
                }
                else
                {
                    File.Delete(mainIni);
                    File.Copy(config.INIPath, mainIni);
                }
            }
            else
            {
                ReadFileFromBin(config.INIBin, "custom.ini");
                File.Delete(mainIni);
                File.Move("custom.ini", mainIni);
            }
        }

        private static void RPXCompOrDecomp(string rpxpath, bool comp)
        {
            var prefix = comp ? "-c" : "-d";
            using Process rpxtool = new Process();
            rpxtool.StartInfo.UseShellExecute = false;
            rpxtool.StartInfo.CreateNoWindow = true;
            rpxtool.StartInfo.FileName = Path.Combine(toolsPath, "wiiurpxtool.exe");
            rpxtool.StartInfo.Arguments = $"{prefix} \"{rpxpath}\"";

            rpxtool.Start();
            rpxtool.WaitForExit();
        }

        private static void ReadFileFromBin(byte[] bin, string output)
        {
            File.WriteAllBytes(output, bin);
        }
        private static void Images(GameConfig config)
        {
            bool usetemp = false;
            bool readbin = false;
            try
            {
                //is an image embedded? yes => export them and check for issues
                //no => using path
                if (Directory.Exists(imgPath)) // sanity check
                    Directory.Delete(imgPath, true);

                Directory.CreateDirectory(imgPath);
                //ICON
                List<bool> Images = new List<bool>();
                if (config.TGAIco.ImgBin == null)
                {
                    //use path
                    if (config.TGAIco.ImgPath != null)
                    {
                        Images.Add(true);
                        CopyAndConvertImage(config.TGAIco.ImgPath, Path.Combine(imgPath), false, 128, 128, 32, "iconTex.tga");
                    }
                    else
                    {
                        if (File.Exists(Path.Combine(toolsPath, "iconTex.tga")))
                        {
                            CopyAndConvertImage(Path.Combine(toolsPath, "iconTex.tga"), Path.Combine(imgPath), false, 128, 128, 32, "iconTex.tga");
                            Images.Add(true);
                        }
                        else
                            Images.Add(false);

                    }
                }
                else
                {
                    ReadFileFromBin(config.TGAIco.ImgBin, $"iconTex.{config.TGAIco.extension}");
                    CopyAndConvertImage($"iconTex.{config.TGAIco.extension}", Path.Combine(imgPath), true, 128, 128, 32, "iconTex.tga");
                    Images.Add(true);
                }
                if (config.TGATv.ImgBin == null)
                {
                    //use path
                    if (config.TGATv.ImgPath != null)
                    {
                        Images.Add(true);
                        CopyAndConvertImage(config.TGATv.ImgPath, Path.Combine(imgPath), false, 1280, 720, 24, "bootTvTex.tga");
                        config.TGATv.ImgPath = Path.Combine(imgPath, "bootTvTex.tga");
                    }
                    else
                    {
                        if (File.Exists(Path.Combine(toolsPath, "bootTvTex.png")))
                        {
                            CopyAndConvertImage(Path.Combine(toolsPath, "bootTvTex.png"), Path.Combine(imgPath), false, 1280, 720, 24, "bootTvTex.tga");
                            usetemp = true;
                            Images.Add(true);

                        }
                        else
                        {
                            Images.Add(false);
                        }
                    }
                }
                else
                {
                    ReadFileFromBin(config.TGATv.ImgBin, $"bootTvTex.{config.TGATv.extension}");
                    CopyAndConvertImage($"bootTvTex.{config.TGATv.extension}", Path.Combine(imgPath), true, 1280, 720, 24, "bootTvTex.tga");
                    config.TGATv.ImgPath = Path.Combine(imgPath, "bootTvTex.tga");
                    Images.Add(true);
                    readbin = true;
                }

                //Drc
                if (config.TGADrc.ImgBin == null)
                {
                    //use path
                    if (config.TGADrc.ImgPath != null)
                    {
                        Images.Add(true);
                        CopyAndConvertImage(config.TGADrc.ImgPath, Path.Combine(imgPath), false, 854, 480, 24, "bootDrcTex.tga");
                    }
                    else
                    {
                        if (Images[1])
                        {
                            using Process conv = new Process();

                            if (!mvvm.debug)
                            {
                                conv.StartInfo.UseShellExecute = false;
                                conv.StartInfo.CreateNoWindow = true;
                            }
                            if (usetemp)
                                File.Copy(Path.Combine(toolsPath, "bootTvTex.png"), Path.Combine(tempPath, "bootDrcTex.png"));
                            else
                            {
                                conv.StartInfo.FileName = Path.Combine(toolsPath, "tga2png.exe");
                                if (!readbin)
                                    conv.StartInfo.Arguments = $"-i \"{config.TGATv.ImgPath}\" -o \"{Path.Combine(tempPath)}\"";
                                else
                                {
                                    if (config.TGATv.extension.Contains("tga"))
                                    {
                                        ReadFileFromBin(config.TGATv.ImgBin, $"bootTvTex.{config.TGATv.extension}");
                                        conv.StartInfo.Arguments = $"-i \"bootTvTex.{config.TGATv.extension}\" -o \"{Path.Combine(tempPath)}\"";
                                    }
                                    else
                                        ReadFileFromBin(config.TGATv.ImgBin, Path.Combine(tempPath, "bootTvTex.png"));
                                }

                                if (!readbin || config.TGATv.extension.Contains("tga"))
                                {
                                    conv.Start();
                                    conv.WaitForExit();
                                }

                                File.Copy(Path.Combine(tempPath, "bootTvTex.png"), Path.Combine(tempPath, "bootDrcTex.png"));

                                if (File.Exists(Path.Combine(tempPath, "bootTvTex.png")))
                                    File.Delete(Path.Combine(tempPath, "bootTvTex.png"));

                                if (File.Exists($"bootTvTex.{config.TGATv.extension}"))
                                    File.Delete($"bootTvTex.{config.TGATv.extension}");
                            }

                            CopyAndConvertImage(Path.Combine(tempPath, "bootDrcTex.png"), Path.Combine(imgPath), false, 854, 480, 24, "bootDrcTex.tga");
                            Images.Add(true);
                        }
                        else
                            Images.Add(false);
                    }
                }
                else
                {
                    ReadFileFromBin(config.TGADrc.ImgBin, $"bootDrcTex.{config.TGADrc.extension}");
                    CopyAndConvertImage($"bootDrcTex.{config.TGADrc.extension}", Path.Combine(imgPath), true, 854, 480, 24, "bootDrcTex.tga");
                    Images.Add(true);
                }

                //tv



                //logo
                if (config.TGALog.ImgBin == null)
                    //use path
                    if (config.TGALog.ImgPath != null)
                    {
                        Images.Add(true);
                        CopyAndConvertImage(config.TGALog.ImgPath, Path.Combine(imgPath), false, 170, 42, 32, "bootLogoTex.tga");
                    }
                    else
                        Images.Add(false);
                else
                {
                    ReadFileFromBin(config.TGALog.ImgBin, $"bootLogoTex.{config.TGALog.extension}");
                    CopyAndConvertImage($"bootLogoTex.{config.TGALog.extension}", Path.Combine(imgPath), true, 170, 42, 32, "bootLogoTex.tga");
                    Images.Add(true);
                }

                //Fixing Images + Injecting them
                if (Images[0] || Images[1] || Images[2] || Images[3])
                {
                    using (Process checkIfIssue = new Process())
                    {
                        checkIfIssue.StartInfo.UseShellExecute = false;
                        checkIfIssue.StartInfo.CreateNoWindow = false;
                        checkIfIssue.StartInfo.RedirectStandardOutput = true;
                        checkIfIssue.StartInfo.RedirectStandardError = true;
                        checkIfIssue.StartInfo.FileName = $"{Path.Combine(toolsPath, "tga_verify.exe")}";
                        Console.WriteLine(Directory.GetCurrentDirectory());
                        checkIfIssue.StartInfo.Arguments = $"\"{imgPath}\"";
                        checkIfIssue.Start();
                        checkIfIssue.WaitForExit();
                        var s = checkIfIssue.StandardOutput.ReadToEnd();

                        if (s.Contains("width") || s.Contains("height") || s.Contains("depth"))
                            throw new Exception("Size");

                        var e = checkIfIssue.StandardError.ReadToEnd();

                        if (e.Contains("width") || e.Contains("height") || e.Contains("depth"))
                            throw new Exception("Size");

                        if (e.Contains("TRUEVISION") || s.Contains("TRUEVISION"))
                        {
                            checkIfIssue.StartInfo.UseShellExecute = false;
                            checkIfIssue.StartInfo.CreateNoWindow = false;
                            checkIfIssue.StartInfo.RedirectStandardOutput = true;
                            checkIfIssue.StartInfo.RedirectStandardError = true;
                            checkIfIssue.StartInfo.FileName = $"{Path.Combine(toolsPath, "tga_verify.exe")}";
                            Console.WriteLine(Directory.GetCurrentDirectory());
                            checkIfIssue.StartInfo.Arguments = $"--fixup \"{imgPath}\"";
                            checkIfIssue.Start();
                            checkIfIssue.WaitForExit();
                        }
                        // Console.ReadLine();
                    }

                    if (Images[1])
                    {
                        File.Delete(Path.Combine(baseRomPath, "meta", "bootTvTex.tga"));
                        File.Move(Path.Combine(imgPath, "bootTvTex.tga"), Path.Combine(baseRomPath, "meta", "bootTvTex.tga"));
                    }
                    if (Images[2])
                    {
                        File.Delete(Path.Combine(baseRomPath, "meta", "bootDrcTex.tga"));
                        File.Move(Path.Combine(imgPath, "bootDrcTex.tga"), Path.Combine(baseRomPath, "meta", "bootDrcTex.tga"));
                    }
                    if (Images[0])
                    {
                        File.Delete(Path.Combine(baseRomPath, "meta", "iconTex.tga"));
                        File.Move(Path.Combine(imgPath, "iconTex.tga"), Path.Combine(baseRomPath, "meta", "iconTex.tga"));
                    }
                    if (Images[3])
                    {
                        File.Delete(Path.Combine(baseRomPath, "meta", "bootLogoTex.tga"));
                        File.Move(Path.Combine(imgPath, "bootLogoTex.tga"), Path.Combine(baseRomPath, "meta", "bootLogoTex.tga"));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);

                if (e.Message.Contains("Size"))
                    throw e;

                throw new Exception("Images");
            }
        }

        private static void PrepareImageDirectory()
        {
            if (Directory.Exists(imgPath))
                Directory.Delete(imgPath, true);

            Directory.CreateDirectory(imgPath);
        }

        private static bool HandleImage(PNGTGA imgConfig, string fileName, string outputDir, int width, int height, int bitDepth)
        {
            if (imgConfig.ImgBin == null)
            {
                if (!string.IsNullOrEmpty(imgConfig.ImgPath))
                {
                    CopyAndConvertImage(imgConfig.ImgPath, outputDir, false, width, height, bitDepth, $"{fileName}.tga");
                    return true;
                }
                else if (File.Exists(Path.Combine(toolsPath, $"{fileName}.tga")))
                {
                    CopyAndConvertImage(Path.Combine(toolsPath, $"{fileName}.tga"), outputDir, false, width, height, bitDepth, $"{fileName}.tga");
                    return true;
                }
            }
            else
            {
                ReadFileFromBin(imgConfig.ImgBin, $"{fileName}.{imgConfig.extension}");
                CopyAndConvertImage($"{fileName}.{imgConfig.extension}", outputDir, true, width, height, bitDepth, $"{fileName}.tga");
                return true;
            }

            // When tf would this ever return false?
            return false;
        }

        private static bool HandleDrcImage(GameConfig config, bool hasTvImage)
        {
            if (config.TGADrc.ImgBin == null)
            {
                if (!string.IsNullOrEmpty(config.TGADrc.ImgPath))
                {
                    CopyAndConvertImage(config.TGADrc.ImgPath, imgPath, false, 854, 480, 24, "bootDrcTex.tga");
                    return true;
                }
                else if (hasTvImage)
                {
                    ConvertTvImageToDrc(config);
                    return true;
                }
            }
            else
            {
                ReadFileFromBin(config.TGADrc.ImgBin, $"bootDrcTex.{config.TGADrc.extension}");
                CopyAndConvertImage($"bootDrcTex.{config.TGADrc.extension}", imgPath, true, 854, 480, 24, "bootDrcTex.tga");
                return true;
            }
            return false;
        }

        private static void ConvertTvImageToDrc(GameConfig config)
        {        
            string tempFilePath = Path.Combine(tempPath, "bootDrcTex.png");

            if (config.TGATv.extension.Contains("tga"))
                using (var process = new Process())
                {
                    if (!mvvm.debug)
                    {
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;
                    }

                    ReadFileFromBin(config.TGATv.ImgBin, $"bootTvTex.{config.TGATv.extension}");
                    process.StartInfo.Arguments = $"-i \"bootTvTex.{config.TGATv.extension}\" -o \"{tempFilePath}\"";

                    process.Start();
                    process.WaitForExit();
                }
            else
                ReadFileFromBin(config.TGATv.ImgBin, Path.Combine(tempPath, "bootTvTex.png"));

            CopyAndConvertImage(tempFilePath, imgPath, false, 854, 480, 24, "bootDrcTex.tga");
        }

        private static void VerifyAndInjectImages(bool hasIconImage, bool hasTvImage, bool hasDrcImage, bool hasLogoImage)
        {
            using (Process checkIfIssue = new Process())
            {
                checkIfIssue.StartInfo.UseShellExecute = false;
                checkIfIssue.StartInfo.CreateNoWindow = true;
                checkIfIssue.StartInfo.RedirectStandardOutput = true;
                checkIfIssue.StartInfo.RedirectStandardError = true;
                checkIfIssue.StartInfo.FileName = $"{Path.Combine(toolsPath, "tga_verify.exe")}";
                checkIfIssue.StartInfo.Arguments = $"\"{imgPath}\"";
                checkIfIssue.Start();
                checkIfIssue.WaitForExit();

                string output = checkIfIssue.StandardOutput.ReadToEnd();
                string error = checkIfIssue.StandardError.ReadToEnd();

                if (output.Contains("width") || output.Contains("height") || output.Contains("depth") ||
                    error.Contains("width") || error.Contains("height") || error.Contains("depth"))
                {
                    throw new Exception("Size");
                }

                if (output.Contains("TRUEVISION") || error.Contains("TRUEVISION"))
                {
                    checkIfIssue.StartInfo.Arguments = $"--fixup \"{imgPath}\"";
                    checkIfIssue.Start();
                    checkIfIssue.WaitForExit();
                }
            }

            MoveProcessedImages(hasIconImage, hasTvImage, hasDrcImage, hasLogoImage);
        }

        private static void MoveProcessedImages(bool hasIconImage, bool hasTvImage, bool hasDrcImage, bool hasLogoImage)
        {
            if (hasTvImage)
            {
                string destPath = Path.Combine(baseRomPath, "meta", "bootTvTex.tga");

                if (File.Exists(destPath))
                    File.Delete(destPath);

                File.Move(Path.Combine(imgPath, "bootTvTex.tga"), destPath);
            }

            if (hasDrcImage)
            {
                string destPath = Path.Combine(baseRomPath, "meta", "bootDrcTex.tga");

                if (File.Exists(destPath))
                    File.Delete(destPath);

                File.Move(Path.Combine(imgPath, "bootDrcTex.tga"), destPath);
            }

            if (hasIconImage)
            {
                string destPath = Path.Combine(baseRomPath, "meta", "iconTex.tga");

                if (File.Exists(destPath))
                    File.Delete(destPath);

                File.Move(Path.Combine(imgPath, "iconTex.tga"), destPath);
            }

            if (hasLogoImage)
            {
                string destPath = Path.Combine(baseRomPath, "meta", "bootLogoTex.tga");

                if (File.Exists(destPath))
                    File.Delete(destPath);

                File.Move(Path.Combine(imgPath, "bootLogoTex.tga"), destPath);
            }
        }
        private static string ResolveExpectedFile(string expectedPath)
        {
            // 1. First try the expected path
            if (File.Exists(expectedPath))
                return expectedPath;

            // 2. Try the WindowsToHostPosix version
            var posix = ToolRunner.WindowsToHostPosix(expectedPath);
            if (File.Exists(posix))
                return posix;

            // 3. Try the reverse (host->windows)
            var win = ToolRunner.ToWindowsView(expectedPath);
            if (File.Exists(win))
                return win;

            // 4. Finally, search in ToolsPath + TempPath
            var file = Path.GetFileName(expectedPath);

            var toolsCandidate = Path.Combine(JsonSettingsManager.Settings.ToolsPath, file);
            if (File.Exists(toolsCandidate))
                return toolsCandidate;

            var tempCandidate = Path.Combine(JsonSettingsManager.Settings.TempPath, file);
            if (File.Exists(tempCandidate))
                return tempCandidate;

            return null;
        }

        private static void CopyAndConvertImage(string inputPath, string outputPath, bool delete, int widht, int height, int bit, string newname)
        {
            inputPath = ResolveExpectedFile(inputPath);

            if (inputPath.EndsWith(".tga"))
                File.Copy(inputPath, Path.Combine(outputPath,newname));
            else
            {
                using (Process png2tga = new Process())
                {
                    png2tga.StartInfo.UseShellExecute = false;
                    png2tga.StartInfo.CreateNoWindow = true;
                    var extension = new FileInfo(inputPath).Extension;

                    if (extension.Contains("png"))
                        png2tga.StartInfo.FileName = Path.Combine(toolsPath, "png2tga.exe");
                    else if (extension.Contains("jpg") || extension.Contains("jpeg"))
                        png2tga.StartInfo.FileName = Path.Combine(toolsPath, "jpg2tga.exe");
                    else if (extension.Contains("bmp"))
                        png2tga.StartInfo.FileName = Path.Combine(toolsPath, "bmp2tga.exe");
                    
                    png2tga.StartInfo.Arguments = $"-i \"{inputPath}\" -o \"{outputPath}\" --width={widht} --height={height} --tga-bpp={bit} --tga-compression=none";

                    png2tga.Start();
                    png2tga.WaitForExit();
                }
                string name = Path.GetFileNameWithoutExtension(inputPath);

                if(File.Exists(Path.Combine(outputPath , name + ".tga")))
                    File.Move(Path.Combine(outputPath, name + ".tga"), Path.Combine(outputPath, newname));
            }
            if (delete)
                File.Delete(inputPath);
        }

        private static string RemoveHeader(string filePath)
        {
            // logic taken from snesROMUtil
            using FileStream inStream = new FileStream(filePath, FileMode.Open);
            byte[] header = new byte[512];
            inStream.Read(header, 0, 512);
            string string1 = BitConverter.ToString(header, 8, 3);
            string string2 = Encoding.ASCII.GetString(header, 0, 11);
            string string3 = BitConverter.ToString(header, 30, 16);

            if (string1 != "AA-BB-04" && string2 != "GAME DOCTOR" && string3 != "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00")
                return filePath;

            string newFilePath = Path.Combine(tempPath, Path.GetFileName(filePath));
            using (FileStream outStream = new FileStream(newFilePath, FileMode.OpenOrCreate))
            {
                inStream.CopyTo(outStream);
            }

            return newFilePath;
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Use the optimized parallel copy helper for better throughput.
            // It preserves directory structure and copies all files recursively when copySubDirs is true.
            if (!Directory.Exists(sourceDirName))
            {
                Logger.Log($"Source directory does not exist or could not be found: {sourceDirName}");
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDirName}");
            }

            if (!copySubDirs)
            {
                // Only copy top-level files
                Directory.CreateDirectory(destDirName);
                foreach (var f in Directory.GetFiles(sourceDirName))
                    File.Copy(f, Path.Combine(destDirName, Path.GetFileName(f)), true);
                return;
            }

            // Full recursive copy with bounded parallelism (from settings)
            int deg = 6;
            try { deg = Math.Max(1, Math.Min(32, JsonSettingsManager.Settings.FileCopyParallelism)); } catch { }
            IOHelpers.CopyDirectorySync(sourceDirName, destDirName, maxParallel: deg);
        }
    }
}

