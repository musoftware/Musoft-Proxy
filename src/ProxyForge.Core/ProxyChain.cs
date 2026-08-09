using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ProxyForge.Core
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
        /// Creates an <see cref="HttpClientHandler"/> configured to tunnel through the primary proxy in the chain.
        /// </summary>
        /// <returns>A configured <see cref="HttpClientHandler"/>.</returns>
        public HttpClientHandler CreateHandler()
        {
            if (Nodes == null || Nodes.Count == 0)
            {
                return new HttpClientHandler();
            }

            // Entry node for the chain
            ProxyInfo entryNode = Nodes[0];
            return entryNode.CreateHandler();
        }
    }
}
