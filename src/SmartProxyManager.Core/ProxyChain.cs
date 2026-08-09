using System;
using System.Collections.Generic;
using System.Net.Http;

namespace SmartProxyManager.Core
{
    /// <summary>
    /// Represents a multi-hop chained sequence of proxy nodes.
    /// </summary>
    public class ProxyChain
    {
        /// <summary>
        /// Gets or sets the ordered sequence of proxy nodes forming the chain.
        /// </summary>
        public List<ProxyInfo> Nodes { get; set; } = new List<ProxyInfo>();

        /// <summary>
        /// Creates an <see cref="HttpClientHandler"/> configured to tunnel through the multi-hop proxy chain.
        /// </summary>
        /// <returns>A configured <see cref="HttpClientHandler"/>.</returns>
        /// <exception cref="NotImplementedException">Multi-hop proxy chaining via HttpClientHandler is scheduled for v2.0.</exception>
        public HttpClientHandler CreateHandler()
        {
            throw new NotImplementedException("Multi-hop proxy chaining via HttpClientHandler is scheduled for v2.0 release.");
        }
    }
}
