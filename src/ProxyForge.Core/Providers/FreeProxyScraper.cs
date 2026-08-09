using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ProxyForge.Core
{
    /// <summary>
    /// Growth provider feature that scrapes free proxies from public online endpoints.
    /// </summary>
    public class FreeProxyScraper : IProxyProvider
    {
        private static readonly string[] Sources = new[]
        {
            "https://api.proxyscrape.com/v2/?request=getproxies&protocol=http&timeout=10000&country=all&ssl=all&anonymity=all",
            "https://raw.githubusercontent.com/TheSpeedX/SOCKS-List/master/http.txt",
            "https://raw.githubusercontent.com/clarketm/proxy-list/master/proxy-list-raw.txt"
        };

        /// <summary>
        /// Asynchronously fetches and parses proxies from public free proxy lists.
        /// </summary>
        /// <returns>A list of <see cref="ProxyInfo"/> instances.</returns>
        public async Task<List<ProxyInfo>> FetchAsync()
        {
            var result = new List<ProxyInfo>();
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            foreach (var url in Sources)
            {
                try
                {
                    string content = await client.GetStringAsync(url).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var parsed = ProxyParser.Parse(content, ProxyType.HTTP);
                        result.AddRange(parsed);
                    }
                }
                catch
                {
                    // Ignore failure per-source and continue
                }
            }

            // Also attempt HTML scraping example if HtmlAgilityPack is available
            try
            {
                var web = new HtmlWeb { Timeout = 5000 };
                var doc = await web.LoadFromWebAsync("https://free-proxy-list.net/").ConfigureAwait(false);
                if (doc != null)
                {
                    var nodes = doc.DocumentNode.SelectNodes("//textarea");
                    if (nodes != null)
                    {
                        foreach (var node in nodes)
                        {
                            var parsed = ProxyParser.Parse(node.InnerText, ProxyType.HTTP);
                            result.AddRange(parsed);
                        }
                    }
                }
            }
            catch
            {
                // Fallback gracefully
            }

            // Deduplicate proxies by Host + Port
            var distinct = result
                .GroupBy(p => $"{p.Host}:{p.Port}")
                .Select(g => g.First())
                .ToList();

            return distinct;
        }

        /// <summary>
        /// Alias method for <see cref="FetchAsync"/>.
        /// </summary>
        public async Task<List<ProxyInfo>> ScrapeAsync()
        {
            return await FetchAsync().ConfigureAwait(false);
        }
    }
}
