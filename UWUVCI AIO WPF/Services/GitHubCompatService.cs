using Newtonsoft.Json;
using Octokit;
using System;
using System.Linq;
using System.Threading.Tasks;
using UWUVCI_AIO_WPF.Models;

namespace UWUVCI_AIO_WPF.Services
{
    public class GitHubCompatService : GitHubBaseService
    {
        // --- Utility mapping ---
        public string GetFileNameForConsole(string consoleKey) => consoleKey switch
        {
            "NES" => "NESCompat.json",
            "SNES" => "SNECompat.json",
            "GBA" => "GBACompat.json",
            "N64" => "N64Compat.json",
            "TG16" => "TG1Compat.json",
            "MSX" => "MSXCompat.json",
            "Wii" => "WiiCompat.json",
            "NDS" => "NDSCompat.json",
            _ => throw new ArgumentException($"Unsupported console: {consoleKey}")
        };

        public async Task<string> SubmitEntryAsync(
            string owner,
            string repo,
            string consoleKey,
            GameCompatEntry baseEntry,
            int? gamepadOpt = null,
            string renderSizeOpt = null,
            string appVersion = "unknown")
        {

            // Check blacklist before continuing
            bool isBlacklisted = await DeviceBlacklistService.IsDeviceBlacklistedAsync(BlackListURL, timeoutMs: 4000);
            if (isBlacklisted)
            {
                // Simulate a generic failure so it looks normal to the end-user.
                // Keep the message vague — do not reveal that they are blacklisted.
                throw new InvalidOperationException("Failed to submit compat entry due to a network error. Please try again later.");
            }

            var client = CreateClient();
            var repository = await RetryAsync(() => client.Repository.Get(owner, repo));
            var mainBranch = repository.DefaultBranch;

            var fileName = GetFileNameForConsole(consoleKey);
            var file = await RetryAsync(() => client.Repository.Content.GetAllContents(owner, repo, fileName));
            var json = file[0].Content;
            var sha = file[0].Sha;

            // Deserialize and alphabetize entries
            string updatedJson = UpdateCompatFile(consoleKey, baseEntry, json, gamepadOpt, renderSizeOpt);

            var branchName = await CreateBranchAsync(client, owner, repo, mainBranch, $"compat-{consoleKey.ToLowerInvariant()}");

            var update = new UpdateFileRequest(
                $"Add compatibility entry: {baseEntry.GameName} ({consoleKey})",
                updatedJson,
                sha,
                branchName);

            await RetryAsync(() => client.Repository.Content.UpdateFile(owner, repo, fileName, update));

            // Compute local hashed fingerprint (Base64 SHA256)
            string submitterFingerprint = null;
            try
            {
                submitterFingerprint = DeviceFingerprint.GetHashedFingerprint();
            }
            catch
            {
                // If fingerprint fails for any reason, keep it null don't want this to break submission.
                submitterFingerprint = null;
            }

            var pr = await RetryAsync(() => client.PullRequest.Create(owner, repo,
                new NewPullRequest($"[Compat] Add {baseEntry.GameName} ({consoleKey})", branchName, mainBranch)
                { Body = BuildPrBody(consoleKey, baseEntry, gamepadOpt, renderSizeOpt, appVersion, submitterFingerprint) }));

            return pr.HtmlUrl;
        }

        private string UpdateCompatFile(string consoleKey, GameCompatEntry baseEntry, string json, int? gamepadOpt, string renderSizeOpt)
        {
            if (consoleKey == "Wii")
            {
                var wrapper = JsonConvert.DeserializeObject<CompatFile<WiiCompatEntry>>(json)
                              ?? new CompatFile<WiiCompatEntry>();

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

                wrapper.Compatibility = wrapper.Compatibility
                    .OrderBy(e => e.GameName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(e => e.GameRegion, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return JsonConvert.SerializeObject(wrapper, Formatting.Indented);
            }
            else if (consoleKey == "NDS")
            {
                var wrapper = JsonConvert.DeserializeObject<CompatFile<NdsCompatEntry>>(json)
                              ?? new CompatFile<NdsCompatEntry>();

                wrapper.Compatibility.Add(new NdsCompatEntry
                {
                    GameName = baseEntry.GameName,
                    GameRegion = baseEntry.GameRegion,
                    BaseName = baseEntry.BaseName,
                    BaseRegion = baseEntry.BaseRegion,
                    Status = baseEntry.Status,
                    Notes = baseEntry.Notes,
                    RenderSize = renderSizeOpt ?? "1x"
                });

                wrapper.Compatibility = wrapper.Compatibility
                    .OrderBy(e => e.GameName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(e => e.GameRegion, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return JsonConvert.SerializeObject(wrapper, Formatting.Indented);
            }
            else
            {
                var wrapper = JsonConvert.DeserializeObject<CompatFile<GameCompatEntry>>(json)
                              ?? new CompatFile<GameCompatEntry>();

                wrapper.Compatibility.Add(baseEntry);

                wrapper.Compatibility = wrapper.Compatibility
                    .OrderBy(e => e.GameName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(e => e.GameRegion, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return JsonConvert.SerializeObject(wrapper, Formatting.Indented);
            }
        }

        private string BuildPrBody(string consoleKey, GameCompatEntry entry, int? gamepadOpt, string renderSizeOpt, string appVersion, string fingerprintBase64)
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

            var fingerprintSection = "";
            if (!string.IsNullOrWhiteSpace(fingerprintBase64))
            {
                // Use a clear marker "FP:" so it's easy to search
                fingerprintSection = $"\n- **Submitter fingerprint (FP):** `{fingerprintBase64}`\n";
            }

            return $@"
### 📌 Compatibility Entry Submission

**Submitted via:** UWUVCI V3 App  
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
- App Version: {appVersion}{fingerprintSection}
".TrimStart();
        }
    }
}
