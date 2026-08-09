using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProxyForge.Core
{
    /// <summary>
    /// Provider implementation for fetching private/residential proxies from the Webshare.io REST API.
    /// </summary>
    public class WebShareProxyProvider : IProxyProvider
    {
        private static readonly HttpClient SharedHttpClient = new HttpClient();

        /// <summary>
        /// Gets or sets the Webshare API authorization token.
        /// </summary>
        public string ApiToken { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of pages to fetch (default 5).
        /// </summary>
        public int MaxPages { get; set; } = 5;

        /// <summary>
        /// Gets or sets the page size per request (default 25, max 250).
        /// </summary>
        public int PageSize { get; set; } = 25;

        /// <summary>
        /// Initializes a new instance of <see cref="WebShareProxyProvider"/>.
        /// </summary>
        /// <param name="apiToken">Webshare API Authorization Token.</param>
        public WebShareProxyProvider(string apiToken)
        {
            ApiToken = apiToken ?? throw new ArgumentNullException(nameof(apiToken));
        }

        /// <summary>
        /// Asynchronously fetches proxies from the Webshare API.
        /// </summary>
        /// <returns>A list of parsed <see cref="ProxyInfo"/> objects with host, port, username, and password.</returns>
        public async Task<List<ProxyInfo>> FetchAsync()
        {
            var result = new List<ProxyInfo>();
            if (string.IsNullOrWhiteSpace(ApiToken)) return result;

            for (int page = 1; page <= MaxPages; page++)
            {
                try
                {
                    string url = $"https://proxy.webshare.io/api/v2/proxy/list/?mode=direct&page={page}&page_size={PageSize}";
                    using var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.TryAddWithoutValidation("Authorization", $"Token {ApiToken.Trim()}");

                    using var response = await SharedHttpClient.SendAsync(request).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode) break;

                    string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    using var doc = JsonDocument.Parse(json);

                    if (!doc.RootElement.TryGetProperty("results", out var resultsProp) || resultsProp.ValueKind != JsonValueKind.Array)
                    {
                        break;
                    }

                    int count = 0;
                    foreach (var item in resultsProp.EnumerateArray())
                    {
                        string host = item.GetProperty("proxy_address").GetString() ?? "";
                        int port = item.GetProperty("port").GetInt32();
                        string username = item.GetProperty("username").GetString() ?? "";
                        string password = item.GetProperty("password").GetString() ?? "";
                        string countryCode = item.TryGetProperty("country_code", out var cc) ? (cc.GetString() ?? "Unknown") : "Unknown";

                        if (!string.IsNullOrWhiteSpace(host) && port > 0)
                        {
                            var proxy = new ProxyInfo(host, port, username, password, ProxyType.HTTP)
                            {
                                CountryCode = countryCode
                            };
                            result.Add(proxy);
                            count++;
                        }
                    }

                    if (count == 0) break;
                }
                catch
                {
                    break;
                }
            }

            return result;
        }
    }
}
