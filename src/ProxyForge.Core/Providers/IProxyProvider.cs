using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProxyForge.Core
{
    /// <summary>
    /// Defines a provider mechanism to fetch or load proxies from external sources (files, remote APIs, databases).
    /// </summary>
    public interface IProxyProvider
    {
        /// <summary>
        /// Asynchronously fetches a list of proxies from the configured provider source.
        /// </summary>
        /// <returns>A list of <see cref="ProxyInfo"/> instances.</returns>
        Task<List<ProxyInfo>> FetchAsync();
    }
}
