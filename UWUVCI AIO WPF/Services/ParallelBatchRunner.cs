using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UWUVCI_AIO_WPF.Services
{
    /// <summary>
    /// Generic bounded-parallel runner for small batches. Extracted to be unit testable.
    /// </summary>
    public static class ParallelBatchRunner
    {
        /// <summary>
        /// Runs work for each item with at most <paramref name="maxConcurrency"/> concurrent tasks.
        /// Calls <paramref name="onItemCompleted"/> after each successful completion.
        /// Exceptions are propagated (first) after all tasks finish/abort.
        /// </summary>
        public static async Task RunAsync<T>(
            IEnumerable<T> items,
            Func<T, Task> work,
            Action onItemCompleted,
            int maxConcurrency = 3,
            CancellationToken cancellationToken = default)
        {
            if (items == null) return;
            if (work == null) throw new ArgumentNullException(nameof(work));
            if (maxConcurrency <= 0) maxConcurrency = 1;

            using var gate = new SemaphoreSlim(maxConcurrency);
            var tasks = new List<Task>();

            foreach (var item in items)
            {
                await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await work(item).ConfigureAwait(false);
                        onItemCompleted?.Invoke();
                    }
                    finally
                    {
                        gate.Release();
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}

