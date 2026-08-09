using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyForge.Core
{
    /// <summary>
    /// Provides asynchronous proxy testing and latency measurement utilities.
    /// </summary>
    public static class ProxyTester
    {
        private const string DefaultTestUrl = ProxyConstants.DefaultTestUrl;

        /// <summary>
        /// Asynchronously tests a single proxy for connectivity and measures latency.
        /// </summary>
        /// <param name="proxy">The proxy to test.</param>
        /// <param name="timeoutMs">Timeout in milliseconds (default 7000ms).</param>
        /// <param name="testUrl">The remote URL endpoint to test against (default https://api.ipify.org).</param>
        /// <returns>A <see cref="TestResult"/> containing test metrics.</returns>
        public static async Task<TestResult> TestAsync(ProxyInfo proxy, int timeoutMs = ProxyConstants.DefaultTestTimeoutMs, string testUrl = DefaultTestUrl)
        {
            if (proxy == null) throw new ArgumentNullException(nameof(proxy));

            var result = new TestResult { Proxy = proxy };
            var sw = Stopwatch.StartNew();

            try
            {
                var handler = ProxyFactory.CreateHandler(proxy);
                using var client = new HttpClient(handler, disposeHandler: true);
                client.Timeout = TimeSpan.FromMilliseconds(timeoutMs);

                using var response = await client.GetAsync(testUrl).ConfigureAwait(false);
                sw.Stop();

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    result.IsSuccess = true;
                    result.LatencyMs = (int)sw.ElapsedMilliseconds;
                    result.IP = content.Trim();

                    proxy.IsLive = true;
                    proxy.LatencyMs = result.LatencyMs;
                    proxy.LastChecked = DateTime.UtcNow;
                }
                else
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
                    proxy.IsLive = false;
                    proxy.LatencyMs = -1;
                    proxy.LastChecked = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                proxy.IsLive = false;
                proxy.LatencyMs = -1;
                proxy.LastChecked = DateTime.UtcNow;
            }

            return result;
        }

        /// <summary>
        /// Asynchronously tests a list of proxies in parallel with concurrency limits and progress reporting.
        /// </summary>
        /// <param name="proxies">List of proxies to test.</param>
        /// <param name="maxParallel">Maximum parallel tasks (default 20).</param>
        /// <param name="progress">Progress reporter invoked when each proxy test completes.</param>
        /// <param name="timeoutMs">Timeout in milliseconds per proxy test.</param>
        /// <param name="cancellationToken">Cancellation token to abort remaining tests.</param>
        /// <returns>A task representing the parallel operation.</returns>
        public static async Task TestAllAsync(
            List<ProxyInfo> proxies,
            int maxParallel = 20,
            IProgress<ProxyInfo>? progress = null,
            int timeoutMs = 7000,
            CancellationToken cancellationToken = default)
        {
            if (proxies == null || proxies.Count == 0) return;

            using var semaphore = new SemaphoreSlim(maxParallel, maxParallel);
            var tasks = proxies.Select(async proxy =>
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    await TestAsync(proxy, timeoutMs).ConfigureAwait(false);
                    progress?.Report(proxy);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
