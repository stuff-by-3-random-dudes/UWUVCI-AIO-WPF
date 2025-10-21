using Octokit;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UWUVCI_AIO_WPF.Services
{
    public class GitHubFeedbackService : GitHubBaseService
    {
        public async Task<string> SubmitIssueAsync(
            string owner,
            string repo,
            string type,
            string description,
            string appVersion,
            bool includeSystemInfo = false,
            string logDirectory = null)
        {
            // Blacklist protection
            bool isBlacklisted = await DeviceBlacklistService.IsDeviceBlacklistedAsync(BlackListURL, timeoutMs: 4000);
            if (isBlacklisted)
                return null;

            var client = CreateClient();
            var now = GetTimestampUtc();

            string fingerprint = null;
            try { fingerprint = DeviceFingerprint.GetHashedFingerprint(); } catch { }

            // Optional sections
            string sysInfo = includeSystemInfo ? EnvironmentInfoService.TryGetSummary() : null;
            string latestLog = !string.IsNullOrEmpty(logDirectory) && Directory.Exists(logDirectory)
                ? GetMostRecentLogFile(logDirectory)
                : null;

            string logContent = TryReadLog(latestLog);

            // Build title & body
            var title = $"[{type}] {TruncateTitle(description)}";
            var body = BuildIssueBody(type, description, appVersion, fingerprint, now, sysInfo, logContent);

            // Create issue
            var issue = await RetryAsync(() =>
                client.Issue.Create(owner, repo, new NewIssue(title) { Body = body }));

            return issue.HtmlUrl;
        }

        // ------------------------
        // 🔹 Helpers
        // ------------------------

        private static string GetMostRecentLogFile(string dir)
        {
            try
            {
                var files = new DirectoryInfo(dir).GetFiles("log_*.txt");
                if (files.Length == 0) return null;
                return files.OrderByDescending(f => f.LastWriteTimeUtc).First().FullName;
            }
            catch { return null; }
        }

        private static string TryReadLog(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return null;

                string content = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(content))
                    return "(empty log file)";
                if (content.Length > 50000)
                    content = content.Substring(0, 50000) + "\n... [truncated]";
                return content;
            }
            catch
            {
                return null;
            }
        }

        private static string BuildIssueBody(
            string type,
            string desc,
            string version,
            string fingerprint,
            string timestamp,
            string sysInfo,
            string logContent)
        {
            var fp = string.IsNullOrWhiteSpace(fingerprint) ? "(none)" : $"`{fingerprint}`";
            string sys = string.IsNullOrEmpty(sysInfo) ? "(not included)" : $"\n```\n{sysInfo}\n```";

            // Build collapsible log section if a log is available
            string logSection;
            if (string.IsNullOrEmpty(logContent))
            {
                logSection = "(no log attached)";
            }
            else
            {
                logSection = $@"
<details>
<summary>📜 Click to expand latest log file</summary>
{logContent}    
</details>";
            }

            return $@"
### 📝 Feedback Submitted via UWUVCI AIO

**Type:** {type}  
**Submitted via:** {BotName}  
**Timestamp:** {timestamp}  
**App Version:** {version}  

---

### 🧩 Description
{desc}

---

### 🖥️ System Info
{sys}

---

### 📜 Log File
{logSection}

---

### 🔍 Device Fingerprint
{fp}

*(This report was submitted anonymously.)*
".Trim();
        }

        private static string TruncateTitle(string text)
        {
            if (string.IsNullOrEmpty(text)) return "Feedback";
            text = text.Replace("\n", " ").Replace("\r", " ");
            return text.Length > 70 ? text.Substring(0, 70) + "..." : text;
        }
    }
}
