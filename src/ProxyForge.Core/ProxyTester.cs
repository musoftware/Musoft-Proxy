using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyForge.Core
{
    /// <summary>
    /// Provides ultra-fast asynchronous proxy testing and latency measurement utilities with TCP pre-checking.
    /// </summary>
    public static class ProxyTester
    {
        private const string DefaultTestUrl = ProxyConstants.DefaultTestUrl;

        private static readonly string[] FastTestUrls = new[]
        {
            "http://checkip.amazonaws.com",
            "https://api.ipify.org",
            "http://ip-api.com/line"
        };

        /// <summary>
        /// Performs a fast TCP socket ping to check if the proxy port is open and accepting connections.
        /// </summary>
        /// <param name="host">Proxy host or IP.</param>
        /// <param name="port">Proxy port.</param>
        /// <param name="timeoutMs">Timeout in milliseconds (default 1200ms).</param>
        /// <returns>True if port is open; otherwise false.</returns>
        public static async Task<bool> IsPortOpenAsync(string host, int port, int timeoutMs = 1200)
        {
            if (string.IsNullOrWhiteSpace(host) || port <= 0 || port > 65535) return false;

            try
            {
                using var client = new TcpClient();
#if NET6_0_OR_GREATER || NETCOREAPP
                using var cts = new CancellationTokenSource(timeoutMs);
                await client.ConnectAsync(host, port, cts.Token).ConfigureAwait(false);
                return client.Connected;
#else
                var connectTask = client.ConnectAsync(host, port);
                var delayTask = Task.Delay(timeoutMs);
                var completed = await Task.WhenAny(connectTask, delayTask).ConfigureAwait(false);
                return completed == connectTask && client.Connected;
#endif
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Asynchronously tests a single proxy for connectivity and measures latency.
        /// Uses TCP pre-check to eliminate dead ports in ~1 second.
        /// </summary>
        /// <param name="proxy">The proxy to test.</param>
        /// <param name="timeoutMs">Timeout in milliseconds (default 4000ms per HTTP test).</param>
        /// <param name="testUrl">The remote URL endpoint to test against.</param>
        /// <returns>A <see cref="TestResult"/> containing test metrics.</returns>
        public static async Task<TestResult> TestAsync(ProxyInfo proxy, int timeoutMs = 4000, string testUrl = DefaultTestUrl)
        {
            if (proxy == null) throw new ArgumentNullException(nameof(proxy));

            var result = new TestResult { Proxy = proxy };

            // Step 1: Ultra-fast TCP port pre-check (eliminates 80% dead proxies in ~1s)
            bool isTcpAlive = await IsPortOpenAsync(proxy.Host, proxy.Port, timeoutMs: 1200).ConfigureAwait(false);
            if (!isTcpAlive)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "TCP connection refused or timed out";
                proxy.IsLive = false;
                proxy.LatencyMs = -1;
                proxy.LastChecked = DateTime.UtcNow;
                return result;
            }

            // Step 2: Test HTTP endpoint
            var urlsToTry = new List<string> { testUrl };
            foreach (var fastUrl in FastTestUrls)
            {
                if (!urlsToTry.Contains(fastUrl, StringComparer.OrdinalIgnoreCase))
                    urlsToTry.Add(fastUrl);
            }

            var sw = Stopwatch.StartNew();
            foreach (var targetUrl in urlsToTry)
            {
                sw.Restart();
                try
                {
                    var handler = ProxyFactory.CreateHandler(proxy);
                    using var client = new HttpClient(handler, disposeHandler: true);
                    client.Timeout = TimeSpan.FromMilliseconds(timeoutMs);

                    using var response = await client.GetAsync(targetUrl).ConfigureAwait(false);
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
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    result.ErrorMessage = ex.Message;
                }
            }

            result.IsSuccess = false;
            proxy.IsLive = false;
            proxy.LatencyMs = -1;
            proxy.LastChecked = DateTime.UtcNow;

            return result;
        }

        /// <summary>
        /// Asynchronously tests a list of proxies in parallel with high concurrency limits (default 100) and progress reporting.
        /// </summary>
        /// <param name="proxies">List of proxies to test.</param>
        /// <param name="maxParallel">Maximum parallel tasks (default 100 for high speed).</param>
        /// <param name="progress">Progress reporter invoked when each proxy test completes.</param>
        /// <param name="timeoutMs">Timeout in milliseconds per proxy HTTP test (default 4000ms).</param>
        /// <param name="cancellationToken">Cancellation token to abort remaining tests.</param>
        /// <returns>A task representing the parallel operation.</returns>
        public static async Task TestAllAsync(
            List<ProxyInfo> proxies,
            int maxParallel = 100,
            IProgress<ProxyInfo>? progress = null,
            int timeoutMs = 4000,
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
