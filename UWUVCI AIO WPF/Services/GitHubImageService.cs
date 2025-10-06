using Newtonsoft.Json;
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
        private const string BotName = "UWUVCI-ContriBot";

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

            // --- Build PR body and open PR ---
            var prBody = BuildPrBody(consoleKey, gameName, appVersion, imagePaths);
            var pr = await RetryAsync(() =>
                client.PullRequest.Create(owner, repo,
                    new NewPullRequest($"[Images] {gameName} ({consoleKey})", branchName, baseBranch)
                    {
                        Body = prBody
                    }));

            return pr.HtmlUrl;
        }

        private string BuildPrBody(string consoleKey, string gameName, string appVersion, string[] imagePaths)
        {
            var now = GetTimestampUtc();

            var imageList = string.Join("\n", imagePaths.Select(p => $"- {Path.GetFileName(p)}"));
            if (string.IsNullOrWhiteSpace(imageList))
                imageList = "(none)";

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
- App Version: {appVersion}
".Trim();
        }
    }
}
