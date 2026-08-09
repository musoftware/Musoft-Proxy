using System.Collections.Generic;

namespace SmartProxyManager.Core
{
    /// <summary>
    /// Defines an algorithm for selecting a proxy from an available pool.
    /// </summary>
    public interface IRotationStrategy
    {
        /// <summary>
        /// Selects the next proxy to be used based on the strategy algorithm.
        /// </summary>
        /// <param name="availableProxies">List of eligible proxies.</param>
        /// <param name="currentProxy">The currently active proxy, if any.</param>
        /// <returns>A selected <see cref="ProxyInfo"/> or null if no eligible proxy is available.</returns>
        ProxyInfo? SelectProxy(IReadOnlyList<ProxyInfo> availableProxies, ProxyInfo? currentProxy);
    }
}
