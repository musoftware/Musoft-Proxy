using System;
using System.Collections.Generic;

namespace SmartProxyManager.Core
{
    /// <summary>
    /// Implements a Random selection proxy rotation strategy.
    /// </summary>
    public class RandomStrategy : IRotationStrategy
    {
        private readonly Random _random = new Random();
        private readonly object _lock = new object();

        /// <inheritdoc />
        public ProxyInfo? SelectProxy(IReadOnlyList<ProxyInfo> availableProxies, ProxyInfo? currentProxy)
        {
            if (availableProxies == null || availableProxies.Count == 0)
                return null;

            lock (_lock)
            {
                int index = _random.Next(availableProxies.Count);
                return availableProxies[index];
            }
        }
    }
}
