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
#if DEBUG
            string envToken = "";
            return envToken;
#endif
            // BEGIN_TOKEN_REGION
            throw new InvalidOperationException("Token not injected — build script must run first.");
            // END_TOKEN_REGION
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
