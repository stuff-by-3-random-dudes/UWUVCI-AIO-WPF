using Octokit;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UWUVCI_AIO_WPF.Services
{
    /// <summary>
    /// Shared base class for all UWUVCI GitHub services.
    /// Provides token decoding, GitHubClient creation, retry logic, and helper methods.
    /// </summary>
    public abstract class GitHubBaseService
    {
        // ===============================
        // TOKEN + CLIENT CREATION
        // ===============================
        private string GetToken()
        {
            // ===== COPY INTO GetToken() =====
            byte[] xorKey = new byte[] { 0xE9, 0x3A, 0xEF, 0xEE };
            int[] part1 = new int[] { 179, 8, 135, 153, 177, 11, 159, 183, 191, 80, 161, 218, 188, 124 };
            int[] part2 = new int[] { 159, 182, 191, 9, 169, 134, 141, 9, 162, 153, 191, 87, 139, 131 };
            int[] part3 = new int[] { 188, 81, 151, 172, 188, 9, 186, 222, 191, 87, 186, 219, 167, 84 };
            int[] part4 = new int[] { 136, 220, 164, 80, 173, 143, 184, 86, 135, 187, 167, 123, 210, 211 };

            // ===== Decoding logic =====
            // Combine and decode:
            var all = part1.Concat(part2).Concat(part3).Concat(part4)
                .Select((x, i) => (byte)(x ^ xorKey[i % xorKey.Length])).ToArray();
            return Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(all)));
            // =================================
        }
        public GitHubClient CreateClient()
        {
            var token = GetToken();
            return new GitHubClient(new ProductHeaderValue("UWUVCI-ContriBot"))
            {
                Credentials = new Credentials(token)
            };
        }

        // ===============================
        // COMMON UTILITIES
        // ===============================
        public async Task<T> RetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
        {
            int delay = 1000; // start 1s
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (RateLimitExceededException ex)
                {
                    TimeSpan ra = ex.GetRetryAfterTimeSpan();
                    int waitFor = ra == TimeSpan.Zero
                        ? delay * 5
                        : (int)ra.TotalMilliseconds;

                    await Task.Delay(waitFor);
                }
                catch (HttpRequestException ex) when (attempt < maxRetries)
                {
                    if (ex.Message.Contains("502") || ex.Message.Contains("503") || ex.Message.Contains("504"))
                        await Task.Delay(delay);
                    else
                        throw;
                }
                catch (ApiException ex) when (attempt < maxRetries)
                {
                    if (ex.HttpResponse?.StatusCode == System.Net.HttpStatusCode.BadGateway ||
                        ex.HttpResponse?.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                        ex.HttpResponse?.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
                        await Task.Delay(delay);
                    else
                        throw;
                }

                delay *= 2; // exponential backoff
            }

            throw new Exception("GitHub API operation failed after multiple retries.");
        }

        public async Task RetryAsync(Func<Task> operation, int maxRetries = 3)
            => await RetryAsync(async () => { await operation(); return true; }, maxRetries);

        public async Task<string> CreateBranchAsync(GitHubClient client, string owner, string repo, string baseBranch, string prefix)
        {
            var baseRef = await RetryAsync(() => client.Git.Reference.Get(owner, repo, $"heads/{baseBranch}"));
            string branchName = $"{prefix}-{Guid.NewGuid():N}";
            await RetryAsync(() => client.Git.Reference.Create(owner, repo, new NewReference($"refs/heads/{branchName}", baseRef.Object.Sha)));
            return branchName;
        }

        public string GetTimestampUtc()
            => DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'");
    }
}
