using System;
using OpenQA.Selenium.Chrome;

namespace ProxyForge.Core
{
    /// <summary>
    /// Extension methods for configuring Selenium WebDriver ChromeOptions with ProxyForge proxies.
    /// </summary>
    public static class SeleniumProxyExtension
    {
        /// <summary>
        /// Configures ChromeOptions to route traffic through the specified <see cref="ProxyInfo"/>.
        /// </summary>
        /// <param name="options">The ChromeOptions instance.</param>
        /// <param name="proxy">The proxy to apply.</param>
        public static void UseProxy(this ChromeOptions options, ProxyInfo proxy)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (proxy == null) throw new ArgumentNullException(nameof(proxy));

            string scheme = proxy.Type == ProxyType.SOCKS5 ? "socks5://" : "http://";
            string proxyAddress = $"{scheme}{proxy.Host}:{proxy.Port}";
            options.AddArgument($"--proxy-server={proxyAddress}");
        }
    }
}
