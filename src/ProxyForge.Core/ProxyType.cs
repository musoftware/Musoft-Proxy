using System;

namespace ProxyForge.Core
{
    /// <summary>
    /// Specifies the protocol type of a proxy server.
    /// </summary>
    public enum ProxyType
    {
        /// <summary>
        /// Standard HTTP / HTTPS proxy protocol.
        /// </summary>
        HTTP,

        /// <summary>
        /// SOCKS5 proxy protocol.
        /// </summary>
        SOCKS5
    }
}
