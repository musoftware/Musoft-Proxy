using System;
using System.Collections.Generic;
using System.Threading;

namespace ProxyForge.Core
{
    /// <summary>
    /// Implements a thread-safe Round-Robin proxy selection strategy.
    /// </summary>
    public class RoundRobinStrategy : IRotationStrategy
    {
        private int _index = -1;

        /// <inheritdoc />
        public ProxyInfo? SelectProxy(IReadOnlyList<ProxyInfo> availableProxies, ProxyInfo? currentProxy)
        {
            if (availableProxies == null || availableProxies.Count == 0)
                return null;

            int count = availableProxies.Count;
            int nextIndex = (Interlocked.Increment(ref _index) & int.MaxValue) % count;
            return availableProxies[nextIndex];
        }
    }
}
