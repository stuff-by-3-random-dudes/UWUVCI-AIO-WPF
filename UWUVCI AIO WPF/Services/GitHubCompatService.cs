using Newtonsoft.Json;
using Octokit;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWUVCI_AIO_WPF.Models;

namespace UWUVCI_AIO_WPF.Helpers
{
    public static class GitHubCompatService
    {
        // TODO: Replace with your real storage (obfuscate/DPAPI/etc.)
        private static string GetToken()
        {
            // ====== Paste arrays from generator ======
            byte[] xorKey = new byte[] { 0x5A, 0xC3, 0x1F, 0x77 };

            int[] part1 = new int[] { /* paste from generator */ };
            int[] part2 = new int[] { /* paste from generator */ };
            int[] part3 = new int[] { /* paste from generator */ };
            int[] part4 = new int[] { /* paste from generator */ };
            // Add/remove parts as needed
            // =========================================

            var allParts = new[] { part1, part2, part3, part4 }
                .Where(p => p != null && p.Length > 0)
                .SelectMany(p => p.Select(x => (byte)x))
                .ToArray();

            // Undo XOR
            for (int i = 0; i < allParts.Length; i++)
                allParts[i] = (byte)(allParts[i] ^ xorKey[i % xorKey.Length]);

            var base64 = Encoding.UTF8.GetString(allParts);
            var token = Encoding.UTF8.GetString(Convert.FromBase64String(base64));

            if (!token.StartsWith("ghp_") && !token.StartsWith("gho_"))
                throw new InvalidOperationException("Invalid token — check your pasted arrays or xorKey.");

            return token;
        }

        // Map console → filename and type
        public static string GetFileNameForConsole(string consoleKey) => consoleKey switch
        {
            "NES" => "NESCompat.json",
            "SNES" => "SNESCompat.json",
            "GBA" => "GBACompat.json",
            "N64" => "N64Compat.json",
            "TG16" => "TG16Compat.json",
            "MSX" => "MSXCompat.json",
            "Wii" => "WiiCompat.json",
            "NDS" => "NDSCompat.json",
            _ => throw new ArgumentException($"Unsupported console: {consoleKey}")
        };

        public static async Task<string> SubmitEntryAsync(
            string owner,
            string repo,
            string consoleKey,
            GameCompatEntry baseEntry,
            int? gamepadOpt = null,     // Wii only
            string renderSizeOpt = null,// NDS only
            string appVersion = "unknown"
        )
        {
            var token = GetToken();
            var client = new GitHubClient(new ProductHeaderValue("UWUVCIContriBot"))
            {
                Credentials = new Credentials(token)
            };

            var fileName = GetFileNameForConsole(consoleKey);

            // Get repo + branch
            var repository = await client.Repository.Get(owner, repo);
            var mainBranch = repository.DefaultBranch;
            var mainRef = await client.Git.Reference.Get(owner, repo, $"heads/{mainBranch}");

            // Get JSON file
            var file = await client.Repository.Content.GetAllContents(owner, repo, fileName);
            var json = file[0].Content;
            var sha = file[0].Sha;

            // Deserialize + update JSON
            string updatedJson;
            if (consoleKey == "Wii")
            {
                var wrapper = JsonConvert.DeserializeObject<CompatFile<WiiCompatEntry>>(json) ?? new CompatFile<WiiCompatEntry>();
                wrapper.Compatibility.Add(new WiiCompatEntry
                {
                    GameName = baseEntry.GameName,
                    GameRegion = baseEntry.GameRegion,
                    BaseName = baseEntry.BaseName,
                    BaseRegion = baseEntry.BaseRegion,
                    Status = baseEntry.Status,
                    Notes = baseEntry.Notes,
                    Gamepad = gamepadOpt ?? 0
                });
                updatedJson = JsonConvert.SerializeObject(wrapper, Formatting.Indented);
            }
            else if (consoleKey == "NDS")
            {
                var wrapper = JsonConvert.DeserializeObject<CompatFile<NdsCompatEntry>>(json) ?? new CompatFile<NdsCompatEntry>();
                wrapper.Compatibility.Add(new NdsCompatEntry
                {
                    GameName = baseEntry.GameName,
                    GameRegion = baseEntry.GameRegion,
                    BaseName = baseEntry.BaseName,
                    BaseRegion = baseEntry.BaseRegion,
                    Status = baseEntry.Status,
                    Notes = baseEntry.Notes,
                    RenderSize = renderSizeOpt ?? ""
                });
                updatedJson = JsonConvert.SerializeObject(wrapper, Formatting.Indented);
            }
            else
            {
                var wrapper = JsonConvert.DeserializeObject<CompatFile<GameCompatEntry>>(json) ?? new CompatFile<GameCompatEntry>();
                wrapper.Compatibility.Add(baseEntry);
                updatedJson = JsonConvert.SerializeObject(wrapper, Formatting.Indented);
            }

            // Create branch
            var branchName = $"compat-{consoleKey.ToLower()}-{Guid.NewGuid():N}";
            await client.Git.Reference.Create(owner, repo, new NewReference($"refs/heads/{branchName}", mainRef.Object.Sha));

            // Commit JSON update
            var update = new UpdateFileRequest(
                $"Add compatibility entry: {baseEntry.GameName} ({consoleKey})",
                updatedJson,
                sha,
                branchName);
            await client.Repository.Content.UpdateFile(owner, repo, fileName, update);

            // ---- Build PR body with template ----
            string prBody = BuildPrBody(consoleKey, baseEntry, gamepadOpt, renderSizeOpt, appVersion);

            // Open PR
            var pr = await client.PullRequest.Create(owner, repo, new NewPullRequest(
                $"[Compat] Add {baseEntry.GameName} ({consoleKey})",
                branchName,
                mainBranch)
            {
                Body = prBody
            });

            return pr.HtmlUrl;
        }
        private static string BuildPrBody(
             string consoleKey,
             GameCompatEntry entry,
             int? gamepadOpt,
             string renderSizeOpt,
             string appVersion)
        {
            var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'");

            string extraFields = "";
            if (consoleKey == "Wii")
            {
                extraFields = $"- **GamePad:** {gamepadOpt}\n" +
                              "  - 0 = No Support\n" +
                              "  - 1 = Partial\n" +
                              "  - 2 = Full\n";
            }
            else if (consoleKey == "NDS")
            {
                extraFields = $"- **Render Size:** {renderSizeOpt}\n";
            }

            return
                $@"### 📌 Compatibility Entry Submission

                **Submitted via:** UWUVCI AIO WPF App  
                **Bot:** UWUVCI-ContriBot  

                ---

                ### 🕹️ Game Info
                - **Game Name:** {entry.GameName}
                - **Game Region:** {entry.GameRegion}
                - **Base Title:** {entry.BaseName}
                - **Base Region:** {entry.BaseRegion}
                - **Console:** {consoleKey}

                ---

                ### ✅ Compatibility
                - **Status:** {entry.Status}  
                  - 0 = Doesn't Work  
                  - 1 = Issues  
                  - 2 = Works  

                {extraFields}
                ---

                ### 📝 Notes
                {entry.Notes}

                ---

                ### 🔗 Metadata
                - Generated at: {now}  
                - App Version: {appVersion}
            ";
        }
    }

}
