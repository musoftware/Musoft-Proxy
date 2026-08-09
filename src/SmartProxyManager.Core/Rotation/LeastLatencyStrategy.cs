using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartProxyManager.Core
{
    /// <summary>
    /// Implements a strategy that selects the proxy with the lowest recorded latency.
    /// </summary>
    public class LeastLatencyStrategy : IRotationStrategy
    {
        /// <inheritdoc />
        public ProxyInfo? SelectProxy(IReadOnlyList<ProxyInfo> availableProxies, ProxyInfo? currentProxy)
        {
            if (availableProxies == null || availableProxies.Count == 0)
                return null;

            var eligible = availableProxies
                .Where(p => p.IsLive == true && !p.IsInCooldown && !p.IsBanned && p.LatencyMs >= 0)
                .OrderBy(p => p.LatencyMs)
                .ToList();

            if (eligible.Count > 0)
            {
                return eligible[0];
            }

            // Fallback to first available proxy if no latency records exist yet
            return availableProxies.FirstOrDefault(p => !p.IsInCooldown && !p.IsBanned);
        }
    }
}
