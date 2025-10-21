using Octokit;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UWUVCI_AIO_WPF.Services
{
    public class GitHubFeedbackService : GitHubBaseService
    {

        /// <summary>
        /// Submit an anonymous issue. If type == "Bug Report" and includeSystemInfo == true,
        /// the service will append environment info. If latestLogDir is provided, the newest
        /// log file in that folder will be uploaded as a private Gist and linked in the issue.
        /// </summary>
        public async Task<string> SubmitIssueAsync(
            string owner,
            string repo,
            string type,
            string description,
            string appVersion,
            bool includeSystemInfo = false,
            string latestLogDir = null)
        {

            // Fingerprint blacklist check
            bool isBlacklisted = await DeviceBlacklistService.IsDeviceBlacklistedAsync(BlackListURL, timeoutMs: 4000);
            if (isBlacklisted)
                throw new InvalidOperationException("Network error. Please try again later."); // intentionally vague

            var client = CreateClient();
            var now = GetTimestampUtc();

            // Compute local hashed fingerprint (Base64 SHA256)
            string submitterFingerprint;
            try
            {
                submitterFingerprint = DeviceFingerprint.GetHashedFingerprint();
            }
            catch
            {
                // If fingerprint fails for any reason, keep it null to avoid breaking submission.
                submitterFingerprint = null;
            }

            // Optional sections
            string systemInfoBlock = string.Empty;
            if (includeSystemInfo && string.Equals(type, "Bug Report", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    // Use your Wine-safe helper
                    var summary = EnvironmentInfoService.TryGetSummary();
                    if (!string.IsNullOrWhiteSpace(summary))
                    {
                        systemInfoBlock =
                            "\n---\n\n### 🖥️ System Info\n```\n" +
                            summary.Trim() +
                            "\n```";
                    }
                }
                catch
                {
                    // Never block issue creation
                }
            }

            string logGistLinkBlock = string.Empty;
            if (!string.IsNullOrWhiteSpace(latestLogDir))
            {
                try
                {
                    var gistUrl = await TryUploadLatestLogAsGistAsync(client, latestLogDir);
                    if (!string.IsNullOrWhiteSpace(gistUrl))
                    {
                        logGistLinkBlock =
                            "\n---\n\n### 📄 Latest Log\n" +
                            "A copy of the latest log has been uploaded privately here:\n" +
                            gistUrl;
                    }
                }
                catch
                {
                    // Ignore gist upload failures silently
                }
            }

            var title = $"[{type}] {TruncateTitle(description)}";
            var body = BuildIssueBody(type, description, appVersion, submitterFingerprint, now, systemInfoBlock, logGistLinkBlock);

            var newIssue = new NewIssue(title) { Body = body };
            var issue = await RetryAsync(() => client.Issue.Create(owner, repo, newIssue));

            return issue.HtmlUrl;
        }

        private async Task<string> TryUploadLatestLogAsGistAsync(GitHubClient client, string logDir)
        {
            if (!Directory.Exists(logDir))
                return null;

            var newest = new DirectoryInfo(logDir)
                .EnumerateFiles("log_*.txt", SearchOption.TopDirectoryOnly)
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .FirstOrDefault();

            if (newest == null || !newest.Exists)
                return null;

            // Keep gist readable (<1–2 MB)
            const int maxBytes = 1024 * 1024;
            string content = File.ReadAllText(newest.FullName);
            if (content.Length > maxBytes)
            {
                content = "(truncated to last 1 MB)\n" + content.Substring(content.Length - maxBytes);
            }

            var gist = new NewGist
            {
                Public = false,
                Description = $"UWUVCI V3 Latest Log ({DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC)"
            };
            gist.Files.Add(newest.Name, content);

            var created = await RetryAsync(() => client.Gist.Create(gist));
            return created?.HtmlUrl;
        }

        private string TruncateTitle(string text)
        {
            if (string.IsNullOrEmpty(text)) return "Feedback";
            text = text.Replace("\n", " ").Replace("\r", " ");
            return text.Length > 70 ? text.Substring(0, 70) + "..." : text;
        }

        private string BuildIssueBody(
            string type,
            string description,
            string appVersion,
            string fingerprint,
            string timestamp,
            string systemInfoBlock,
            string logGistLinkBlock)
        {
            string fp = string.IsNullOrWhiteSpace(fingerprint) ? "(none)" : $"`{fingerprint}`";

            return
$@"### 📝 Feedback Submitted via UWUVCI AIO

**Type:** {type}  
**Submitted via:** {BotName}  
**Timestamp:** {timestamp}  
**App Version:** {appVersion}  

---

### 🧩 Description
{description}
{systemInfoBlock}
{logGistLinkBlock}

---

### 🔍 Device Fingerprint
{fp}

*(This report was submitted anonymously.)*";
        }
    }
}
