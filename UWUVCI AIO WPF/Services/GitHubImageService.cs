using Octokit;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWUVCI_AIO_WPF.Services
{
    /// <summary>
    /// Handles all image PR submissions to the UWUVCI repositories.
    /// Uses the shared GitHubBaseService for authentication, client creation, and retries.
    /// </summary>
    public class GitHubImageService : GitHubBaseService
    {
        /// <summary>
        /// Submits or updates a PR with images and an optional INI in one PR.
        /// </summary>
        public async Task<string> SubmitImagesAndIniPrAsync(
            string owner,
            string repo,
            string consoleKey,
            string gameId,
            string gameName,
            string appVersion,
            string[] imagePaths,
            string iniPathOrNull,
            string[] logicalKeys = null,
            bool uploadOnlyIfMissing = true)
        {
            bool isBlacklisted = await DeviceBlacklistService.IsDeviceBlacklistedAsync(BlackListURL, timeoutMs: 4000);
            if (isBlacklisted)
                return null;

            if ((imagePaths == null || imagePaths.Length == 0) && string.IsNullOrWhiteSpace(iniPathOrNull))
                throw new ArgumentException("No files provided for upload.");

            var client = CreateClient();
            var repository = await RetryAsync(() => client.Repository.Get(owner, repo));
            var baseBranch = repository.DefaultBranch;

            var branchName = await CreateBranchAsync(client, owner, repo, baseBranch, $"contrib-{consoleKey}-{gameName}");
            string folderPath = $"{consoleKey.ToLower()}/{gameId}";
            var commitMessage = $"Add/Update files for {gameName} ({consoleKey})";

            // Upload images (prefer PNG; map to canonical names when possible)
            var uploadedImageNames = new System.Collections.Generic.List<string>();
            if (imagePaths != null)
            {
                for (int i = 0; i < imagePaths.Length; i++)
                {
                    var imgPath = imagePaths[i];
                    if (string.IsNullOrWhiteSpace(imgPath) || !File.Exists(imgPath)) continue;

                    string key = (logicalKeys != null && i < logicalKeys.Length) ? logicalKeys[i] : null;
                    string targetName = null;
                    string sourceForUpload = imgPath;
                    var ext = Path.GetExtension(imgPath).ToLowerInvariant();
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        targetName = key + ".png"; // canonical name
                        if (ext == ".tga")
                        {
                            // prefer sibling .png
                            var siblingPng = Path.ChangeExtension(imgPath, ".png");
                            if (File.Exists(siblingPng))
                                sourceForUpload = siblingPng;
                            else
                            {
                                // try expected file name in same folder
                                var candidate = Path.Combine(Path.GetDirectoryName(imgPath) ?? string.Empty, targetName);
                                if (File.Exists(candidate)) sourceForUpload = candidate; else continue; // skip uploading TGA
                            }
                        }
                    }
                    else
                    {
                        // No key mapping; upload only PNGs
                        if (ext == ".tga") continue;
                        targetName = Path.GetFileName(imgPath);
                    }

                    var repoPath = $"{folderPath}/{targetName}";
                    byte[] fileBytes = File.ReadAllBytes(sourceForUpload);
                    string base64 = Convert.ToBase64String(fileBytes);
                    var createReq = new CreateFileRequest(commitMessage, base64, branchName, convertContentToBase64: false);
                    try
                    {
                        var existing = await RetryAsync(() => client.Repository.Content.GetAllContentsByRef(owner, repo, repoPath, baseBranch));
                        if (existing.Count > 0)
                        {
                            if (!uploadOnlyIfMissing)
                            {
                                var updateReq = new UpdateFileRequest(commitMessage, base64, existing[0].Sha, branchName, convertContentToBase64: false);
                                await RetryAsync(() => client.Repository.Content.UpdateFile(owner, repo, repoPath, updateReq));
                                uploadedImageNames.Add(targetName);
                            }
                            // else: skip updates
                        }
                        else
                        {
                            await RetryAsync(() => client.Repository.Content.CreateFile(owner, repo, repoPath, createReq));
                            uploadedImageNames.Add(targetName);
                        }
                    }
                    catch (NotFoundException)
                    {
                        await RetryAsync(() => client.Repository.Content.CreateFile(owner, repo, repoPath, createReq));
                        uploadedImageNames.Add(targetName);
                    }
                }
            }

            // Upload INI (if provided)
            string iniRepoPath = null;
            if (!string.IsNullOrWhiteSpace(iniPathOrNull) && File.Exists(iniPathOrNull))
            {
                iniRepoPath = $"{folderPath}/game.ini";
                string original = File.ReadAllText(iniPathOrNull, Encoding.UTF8);
                string header =
                    "; COMMUNITY_SUBMITTED = 1\r\n" +
                    "; This INI was submitted by a user via UWUVCI.\r\n" +
                    $"; UWUVCI_Version = {appVersion}\r\n" +
                    $"; Submitted_UTC = {GetTimestampUtc()}\r\n" +
                    "; Please verify before treating as official.\r\n\r\n";
                string composed = header + original;
                string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(composed));
                var createReq = new CreateFileRequest(commitMessage, base64, branchName, convertContentToBase64: false);
                try
                {
                    var existing = await RetryAsync(() => client.Repository.Content.GetAllContentsByRef(owner, repo, iniRepoPath, baseBranch));
                    if (existing.Count > 0)
                    {
                        if (!uploadOnlyIfMissing)
                        {
                            var updateReq = new UpdateFileRequest(commitMessage, base64, existing[0].Sha, branchName, convertContentToBase64: false);
                            await RetryAsync(() => client.Repository.Content.UpdateFile(owner, repo, iniRepoPath, updateReq));
                        }
                        else
                        {
                            // skip INI update if we only want to add missing
                            iniRepoPath = null;
                        }
                    }
                    else
                    {
                        await RetryAsync(() => client.Repository.Content.CreateFile(owner, repo, iniRepoPath, createReq));
                    }
                }
                catch (NotFoundException)
                {
                    await RetryAsync(() => client.Repository.Content.CreateFile(owner, repo, iniRepoPath, createReq));
                }
            }

            // PR body
            string fingerprint = null;
            try { fingerprint = DeviceFingerprint.GetHashedFingerprint(); } catch { fingerprint = null; }
            var prBody = BuildCombinedPrBody(consoleKey, gameName, appVersion, uploadedImageNames.ToArray(), iniRepoPath != null ? "game.ini" : null, fingerprint);
            var pr = await RetryAsync(() => client.PullRequest.Create(owner, repo,
                new NewPullRequest($"[Contribution] {gameName} ({consoleKey})", branchName, baseBranch)
                {
                    Body = prBody
                }));

            return pr.HtmlUrl;
        }

        private string BuildCombinedPrBody(string consoleKey, string gameName, string appVersion, string[] imagePaths, string iniFileName, string fingerprintBase64)
        {
            var now = GetTimestampUtc();
            var imageList = (imagePaths == null)
                ? string.Empty
                : string.Join("\n", imagePaths.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => $"- {Path.GetFileName(p)}"));
            if (string.IsNullOrWhiteSpace(imageList)) imageList = "(none)";
            var iniSection = string.IsNullOrWhiteSpace(iniFileName) ? "(none)" : $"- {iniFileName}";
            var fingerprintSection = string.IsNullOrWhiteSpace(fingerprintBase64) ? string.Empty : $"\n- **Submitter fingerprint (FP):** `{fingerprintBase64}`\n";

            return $@"
### 📦 Contribution (Images + Optional INI)

**Submitted via:** UWUVCI V3 App  
**Bot:** {BotName}  

---

### 🕹️ Game Info
- **Console:** {consoleKey}
- **Game Name:** {gameName}

---

### 🖼️ Images
{imageList}

### 📄 INI
{iniSection}

---

### 🔗 Metadata
- Generated at: {now}  
- App Version: {appVersion}{fingerprintSection}
".Trim();
        }
        /// <summary>
        /// Submits or updates a PR with one or more image files (icon, TV, DRC, etc.)
        /// inside the UWUVCI-Images repo under a console/game-specific directory.
        /// </summary>
        public async Task<string> SubmitImagePrAsync(
            string owner,
            string repo,
            string consoleKey,
            string gameId,
            string gameName,
            string appVersion,
            params string[] imagePaths)
        {

            // Check blacklist before continuing
            bool isBlacklisted = await DeviceBlacklistService.IsDeviceBlacklistedAsync(BlackListURL, timeoutMs: 4000);
            if (isBlacklisted)
                return null;

            if (imagePaths == null || imagePaths.Length == 0)
                throw new ArgumentException("No image paths provided for upload.");

            var client = CreateClient();
            var repository = await RetryAsync(() => client.Repository.Get(owner, repo));
            var baseBranch = repository.DefaultBranch;

            // Create a unique branch for this PR
            var branchName = await CreateBranchAsync(client, owner, repo, baseBranch, $"images-{consoleKey}-{gameName}");

            // Folder structure for images in the repo
            string folderPath = $"{consoleKey.ToLower()}/{gameId}";
            var commitMessage = $"Add/Update images for {gameName} ({consoleKey})";

            foreach (var imgPath in imagePaths.Where(File.Exists))
            {
                var fileName = Path.GetFileName(imgPath);
                var repoPath = $"{folderPath}/{fileName}";

                // --- Read and Base64 encode image correctly ---
                byte[] fileBytes = File.ReadAllBytes(imgPath);
                string base64 = Convert.ToBase64String(fileBytes);

                // --- Make sure Octokit does NOT double-encode ---
                var createReq = new CreateFileRequest(commitMessage, base64, branchName, convertContentToBase64: false);

                try
                {
                    var existing = await RetryAsync(() =>
                        client.Repository.Content.GetAllContentsByRef(owner, repo, repoPath, baseBranch));

                    if (existing.Count > 0)
                    {
                        var updateReq = new UpdateFileRequest(commitMessage, base64, existing[0].Sha, branchName, convertContentToBase64: false);
                        await RetryAsync(() =>
                            client.Repository.Content.UpdateFile(owner, repo, repoPath, updateReq));
                    }
                    else
                    {
                        await RetryAsync(() =>
                            client.Repository.Content.CreateFile(owner, repo, repoPath, createReq));
                    }
                }
                catch (NotFoundException)
                {
                    // File doesn't exist yet — create it
                    await RetryAsync(() =>
                        client.Repository.Content.CreateFile(owner, repo, repoPath, createReq));
                }
            }

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

            // --- Build PR body and open PR ---
            var prBody = BuildPrBody(consoleKey, gameName, appVersion, imagePaths, submitterFingerprint);
            var pr = await RetryAsync(() =>
                client.PullRequest.Create(owner, repo,
                    new NewPullRequest($"[Images] {gameName} ({consoleKey})", branchName, baseBranch)
                    {
                        Body = prBody
                    }));

            return pr.HtmlUrl;
        }

        private string BuildPrBody(string consoleKey, string gameName, string appVersion, string[] imagePaths, string fingerprintBase64 = null)
        {
            var now = GetTimestampUtc();

            var imageList = string.Join("\n", imagePaths.Select(p => $"- {Path.GetFileName(p)}"));
            if (string.IsNullOrWhiteSpace(imageList))
                imageList = "(none)";

            var fingerprintSection = "";
            if (!string.IsNullOrWhiteSpace(fingerprintBase64))
            {
                // Use a clear marker "FP:" so it's easy to search
                fingerprintSection = $"\n- **Submitter fingerprint (FP):** `{fingerprintBase64}`\n";
            }

            return $@"
### 🖼️ Image Submission

**Submitted via:** UWUVCI V3 App  
**Bot:** {BotName}  

---

### 🕹️ Game Info
- **Console:** {consoleKey}
- **Game Name:** {gameName}

---

### 📦 Uploaded Files
{imageList}

---

### 🔗 Metadata
- Generated at: {now}  
- App Version: {appVersion}{fingerprintSection}
".Trim();
        }
    }
}
