using System;
using System.Net.Http;

namespace ProxyForge.Core
{
    /// <summary>
    /// Extension methods for retrieving rotating HttpClient instances bound to a ProxyPool.
    /// </summary>
    public static class HttpClientProxyExtension
    {
        /// <summary>
        /// Instantiates an <see cref="HttpClient"/> configured with a rotated proxy from the target <see cref="ProxyPool"/>.
        /// </summary>
        /// <param name="pool">Target proxy pool.</param>
        /// <param name="sessionKey">Optional session key for sticky routing.</param>
        /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
        public static HttpClient UseRotatingProxy(this ProxyPool pool, string? sessionKey = null)
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            return ProxyFactory.CreateClient(pool, sessionKey);
        }
    }
}
