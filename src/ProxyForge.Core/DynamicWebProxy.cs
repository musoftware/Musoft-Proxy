using System;
using System.Net;
using System.Threading;

namespace ProxyForge.Core
{
    /// <summary>
    /// Dynamic <see cref="IWebProxy"/> implementation integrated directly with <see cref="ProxyManager"/> and its rotation strategies.
    /// </summary>
    public class DynamicWebProxy : IWebProxy
    {
        private readonly ProxyManager _manager;
        private static readonly AsyncLocal<ProxyInfo?> _currentProxy = new AsyncLocal<ProxyInfo?>();

        /// <summary>
        /// Initializes a new instance of <see cref="DynamicWebProxy"/>.
        /// </summary>
        /// <param name="manager">Target proxy manager. Defaults to <see cref="ProxyManager.Default"/>.</param>
        public DynamicWebProxy(ProxyManager? manager = null)
        {
            _manager = manager ?? ProxyManager.Default;
        }

        /// <summary>
        /// Gets credentials for authenticating against rotated proxies.
        /// </summary>
        public ICredentials? Credentials
        {
            get
            {
                var proxy = _currentProxy.Value;
                if (proxy != null && !string.IsNullOrEmpty(proxy.Username))
                {
                    return new NetworkCredential(proxy.Username, proxy.Password);
                }

                // Fallback to next proxy if no proxy was selected yet for this async context
                var fallback = _manager.GetNext();
                if (fallback != null && !string.IsNullOrEmpty(fallback.Username))
                {
                    return new NetworkCredential(fallback.Username, fallback.Password);
                }

                return null;
            }
            set { }
        }

        /// <summary>
        /// Returns the proxy URI for the specified destination endpoint.
        /// </summary>
        /// <param name="destination">Target URI destination.</param>
        /// <returns>Rotated proxy URI or null if disabled/empty.</returns>
        public Uri? GetProxy(Uri destination)
        {
            if (!_manager.IsEnabled) return null;

            var proxy = _manager.GetNext();
            _currentProxy.Value = proxy;

            if (proxy == null) return null;

            string scheme = proxy.Type == ProxyType.SOCKS5 ? "socks5" : "http";
            return new Uri($"{scheme}://{proxy.Host}:{proxy.Port}");
        }

        /// <summary>
        /// Determines whether to bypass the proxy server for the specified host.
        /// </summary>
        /// <param name="host">Target host URI.</param>
        /// <returns>Always false.</returns>
        public bool IsBypassed(Uri host) => false;
    }
}
