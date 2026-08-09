using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyForge.Core
{
    /// <summary>
    /// Event arguments for health check status updates.
    /// </summary>
    public class ProxyHealthEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the evaluated proxy.
        /// </summary>
        public ProxyInfo Proxy { get; }

        /// <summary>
        /// Gets the test result metrics.
        /// </summary>
        public TestResult Result { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="ProxyHealthEventArgs"/>.
        /// </summary>
        public ProxyHealthEventArgs(ProxyInfo proxy, TestResult result)
        {
            Proxy = proxy;
            Result = result;
        }
    }

    /// <summary>
    /// Provides active and continuous health monitoring for proxy pools.
    /// </summary>
    public class ProxyHealthChecker
    {
        private readonly ProxyManager? _manager;

        /// <summary>
        /// Occurs when a proxy is evaluated as live.
        /// </summary>
        public event EventHandler<ProxyHealthEventArgs>? OnProxyAlive;

        /// <summary>
        /// Occurs when a proxy fails health check and is evaluated as dead.
        /// </summary>
        public event EventHandler<ProxyHealthEventArgs>? OnProxyDead;

        /// <summary>
        /// Initializes a new instance of <see cref="ProxyHealthChecker"/>.
        /// </summary>
        /// <param name="manager">Optional associated ProxyManager instance.</param>
        public ProxyHealthChecker(ProxyManager? manager = null)
        {
            _manager = manager;
        }

        /// <summary>
        /// Asynchronously tests a single proxy and fires health events.
        /// </summary>
        /// <param name="proxy">The proxy to test.</param>
        /// <returns>A <see cref="TestResult"/> instance.</returns>
        public async Task<TestResult> CheckAsync(ProxyInfo proxy)
        {
            var result = await ProxyTester.TestAsync(proxy, timeoutMs: 7000).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                OnProxyAlive?.Invoke(this, new ProxyHealthEventArgs(proxy, result));
            }
            else
            {
                OnProxyDead?.Invoke(this, new ProxyHealthEventArgs(proxy, result));
            }

            return result;
        }

        /// <summary>
        /// Asynchronously tests all proxies in parallel.
        /// </summary>
        /// <param name="proxies">List of proxies.</param>
        /// <param name="maxParallel">Maximum parallel tasks.</param>
        /// <param name="progress">Progress reporter.</param>
        public async Task CheckAllAsync(List<ProxyInfo> proxies, int maxParallel = 20, IProgress<ProxyInfo>? progress = null)
        {
            var innerProgress = new Progress<ProxyInfo>(p =>
            {
                progress?.Report(p);
                if (p.IsLive == true)
                {
                    OnProxyAlive?.Invoke(this, new ProxyHealthEventArgs(p, new TestResult { IsSuccess = true, LatencyMs = p.LatencyMs, Proxy = p }));
                }
                else
                {
                    OnProxyDead?.Invoke(this, new ProxyHealthEventArgs(p, new TestResult { IsSuccess = false, Proxy = p }));
                }
            });

            await ProxyTester.TestAllAsync(proxies, maxParallel: maxParallel, progress: innerProgress).ConfigureAwait(false);
        }

        /// <summary>
        /// Starts periodic background auto-checking for a list of proxies.
        /// </summary>
        /// <param name="proxies">Target list of proxies.</param>
        /// <param name="interval">Time interval between checks.</param>
        /// <param name="cancellationToken">Cancellation token to halt background execution.</param>
        public void StartAutoCheck(List<ProxyInfo> proxies, TimeSpan interval, CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await CheckAllAsync(proxies, maxParallel: 10).ConfigureAwait(false);
                        await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"ProxyHealthChecker auto-check error: {ex.Message}");
                    }
                }
            }, cancellationToken);
        }
    }
}
