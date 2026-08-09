using System;
using System.Net;
using System.Net.Http;

namespace ProxyForge.Core
{
    /// <summary>
    /// Static factory helpers for instantiating pre-configured <see cref="HttpClient"/> instances.
    /// </summary>
    public static class ProxyFactory
    {
        /// <summary>
        /// Creates an <see cref="HttpClientHandler"/> configured for the specified <see cref="ProxyInfo"/>.
        /// </summary>
        /// <param name="proxy">Target proxy instance.</param>
        /// <returns>A configured <see cref="HttpClientHandler"/>.</returns>
        public static HttpClientHandler CreateHandler(this ProxyInfo proxy)
        {
            if (proxy == null) throw new ArgumentNullException(nameof(proxy));

            var handler = new HttpClientHandler();
            if (proxy.Type == ProxyType.SOCKS5)
            {
                if (string.IsNullOrEmpty(proxy.Username))
                {
                    handler.Proxy = new MihaZupan.HttpToSocks5Proxy(proxy.Host, proxy.Port);
                }
                else
                {
                    handler.Proxy = new MihaZupan.HttpToSocks5Proxy(proxy.Host, proxy.Port, proxy.Username, proxy.Password);
                }
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

            return handler;
        }

        /// <summary>
        /// Creates an <see cref="HttpClient"/> configured to route requests through a specific <see cref="ProxyInfo"/>.
        /// </summary>
        /// <param name="proxy">Target proxy instance.</param>
        /// <returns>A new <see cref="HttpClient"/>.</returns>
        public static HttpClient CreateClient(ProxyInfo proxy)
        {
            if (proxy == null) throw new ArgumentNullException(nameof(proxy));
            return new HttpClient(CreateHandler(proxy), disposeHandler: true);
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
