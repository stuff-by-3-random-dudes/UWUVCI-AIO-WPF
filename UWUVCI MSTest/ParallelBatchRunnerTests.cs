using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UWUVCI_AIO_WPF.Services;

namespace UWUVCI_MSTest
{
    [TestClass]
    public class ParallelBatchRunnerTests
    {
        [TestMethod]
        public async Task RunAsync_RespectsMaxConcurrency_AndReportsProgress()
        {
            var items = new List<int> { 1, 2, 3, 4, 5 };
            int concurrent = 0;
            int maxObserved = 0;
            int progressCount = 0;

            async Task Work(int i)
            {
                var now = Interlocked.Increment(ref concurrent);
                if (now > maxObserved) Interlocked.Exchange(ref maxObserved, now);
                await Task.Delay(50);
                Interlocked.Decrement(ref concurrent);
            }

            await ParallelBatchRunner.RunAsync(
                items,
                Work,
                onItemCompleted: () => Interlocked.Increment(ref progressCount),
                maxConcurrency: 2
            );

            Assert.IsTrue(maxObserved <= 2, $"Observed concurrency {maxObserved} exceeds limit 2");
            Assert.AreEqual(items.Count, progressCount, "Progress count should equal item count");
        }
    }
}

