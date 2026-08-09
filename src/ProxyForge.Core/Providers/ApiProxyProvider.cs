using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProxyForge.Core
{
    /// <summary>
    /// Proxy provider that fetches raw proxy lists from a remote HTTP API endpoint.
    /// </summary>
    public class ApiProxyProvider : IProxyProvider
    {
        /// <summary>
        /// Gets or sets the target API URL.
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// Gets or sets default proxy type.
        /// </summary>
        public ProxyType DefaultType { get; set; } = ProxyType.HTTP;

        /// <summary>
        /// Initializes a new instance of <see cref="ApiProxyProvider"/>.
        /// </summary>
        /// <param name="apiUrl">Target API URL.</param>
        /// <param name="defaultType">Default proxy type.</param>
        public ApiProxyProvider(string apiUrl, ProxyType defaultType = ProxyType.HTTP)
        {
            ApiUrl = apiUrl;
            DefaultType = defaultType;
        }

        /// <inheritdoc />
        public async Task<List<ProxyInfo>> FetchAsync()
        {
            if (string.IsNullOrEmpty(ApiUrl))
            {
                return new List<ProxyInfo>();
            }

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            string content = await client.GetStringAsync(ApiUrl).ConfigureAwait(false);
            return ProxyParser.Parse(content, DefaultType);
        }
    }
}
