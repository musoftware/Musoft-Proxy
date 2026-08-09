using System;
using System.Net;
using System.Net.Http;

namespace SmartProxyManager.Core
{
    /// <summary>
    /// Static factory helpers for instantiating pre-configured <see cref="HttpClient"/> instances.
    /// </summary>
    public static class ProxyFactory
    {
        /// <summary>
        /// Creates an <see cref="HttpClient"/> configured to route requests through a specific <see cref="ProxyInfo"/>.
        /// </summary>
        /// <param name="proxy">Target proxy instance.</param>
        /// <returns>A new <see cref="HttpClient"/>.</returns>
        public static HttpClient CreateClient(ProxyInfo proxy)
        {
            if (proxy == null) throw new ArgumentNullException(nameof(proxy));

            var handler = new HttpClientHandler();
            if (proxy.Type == ProxyType.SOCKS5)
            {
                handler.Proxy = new MihaZupan.HttpToSocks5Proxy(
                    proxy.Host, proxy.Port,
                    string.IsNullOrEmpty(proxy.Username) ? null : proxy.Username,
                    string.IsNullOrEmpty(proxy.Password) ? null : proxy.Password);
            }
            else
            {
                var webProxy = new WebProxy(proxy.Host, proxy.Port);
                if (!string.IsNullOrEmpty(proxy.Username))
                {
                    webProxy.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
                }
                handler.Proxy = webProxy;
            }

            return new HttpClient(handler, disposeHandler: true);
        }

        /// <summary>
        /// Creates an <see cref="HttpClient"/> using the current rotated proxy resolved from a <see cref="ProxyPool"/>.
        /// </summary>
        /// <param name="pool">Target proxy pool.</param>
        /// <returns>A new <see cref="HttpClient"/>.</returns>
        public static HttpClient CreateClient(ProxyPool pool)
        {
            return CreateClient(pool, sessionKey: null);
        }

        /// <summary>
        /// Creates an <see cref="HttpClient"/> using a sticky session key resolved from a <see cref="ProxyPool"/>.
        /// </summary>
        /// <param name="pool">Target proxy pool.</param>
        /// <param name="sessionKey">Session key for sticky routing.</param>
        /// <returns>A new <see cref="HttpClient"/>.</returns>
        public static HttpClient CreateClient(ProxyPool pool, string? sessionKey)
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));

            ProxyInfo? proxy = pool.GetProxy(sessionKey);
            if (proxy == null)
            {
                return new HttpClient();
            }

            return CreateClient(proxy);
        }
    }
}
