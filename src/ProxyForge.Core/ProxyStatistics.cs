using System.Collections.Concurrent;
using System.Threading;

namespace ProxyForge.Core
{
    /// <summary>
    /// Holds request statistics for a specific proxy node.
    /// </summary>
    public class ProxyNodeStats
    {
        private int _totalRequests;
        private int _successRequests;
        private int _failedRequests;

        /// <summary>
        /// Gets total requests routed through this proxy.
        /// </summary>
        public int TotalRequests => _totalRequests;

        /// <summary>
        /// Gets successful requests.
        /// </summary>
        public int SuccessRequests => _successRequests;

        /// <summary>
        /// Gets failed requests.
        /// </summary>
        public int FailedRequests => _failedRequests;

        internal void RecordSuccess()
        {
            Interlocked.Increment(ref _totalRequests);
            Interlocked.Increment(ref _successRequests);
        }

        internal void RecordFailure()
        {
            Interlocked.Increment(ref _totalRequests);
            Interlocked.Increment(ref _failedRequests);
        }
    }

    /// <summary>
    /// Thread-safe tracker recording request metrics, success/failure counts, and per-proxy usage statistics.
    /// </summary>
    public class ProxyStatistics
    {
        private int _totalRequests;
        private int _successRequests;
        private int _failedRequests;

        private readonly ConcurrentDictionary<string, ProxyNodeStats> _nodeStats = new ConcurrentDictionary<string, ProxyNodeStats>();

        /// <summary>
        /// Gets total requests tracked.
        /// </summary>
        public int TotalRequests => _totalRequests;

        /// <summary>
        /// Gets total successful requests.
        /// </summary>
        public int Success => _successRequests;

        /// <summary>
        /// Gets total failed requests.
        /// </summary>
        public int Failed => _failedRequests;

        /// <summary>
        /// Gets per-proxy usage statistics mapped by proxy address string.
        /// </summary>
        public ConcurrentDictionary<string, ProxyNodeStats> NodeStats => _nodeStats;

        /// <summary>
        /// Records a successful request operation for a given proxy.
        /// </summary>
        /// <param name="proxy">The proxy used.</param>
        public void RecordSuccess(ProxyInfo proxy)
        {
            Interlocked.Increment(ref _totalRequests);
            Interlocked.Increment(ref _successRequests);

            if (proxy != null)
            {
                var stats = _nodeStats.GetOrAdd(proxy.ToString(), _ => new ProxyNodeStats());
                stats.RecordSuccess();
            }
        }

        /// <summary>
        /// Records a failed request operation for a given proxy.
        /// </summary>
        /// <param name="proxy">The proxy used.</param>
        public void RecordFailure(ProxyInfo proxy)
        {
            Interlocked.Increment(ref _totalRequests);
            Interlocked.Increment(ref _failedRequests);

            if (proxy != null)
            {
                var stats = _nodeStats.GetOrAdd(proxy.ToString(), _ => new ProxyNodeStats());
                stats.RecordFailure();
            }
        }
    }
}
