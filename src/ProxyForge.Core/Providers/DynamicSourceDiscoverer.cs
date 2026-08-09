using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ProxyForge.Core
{
    /// <summary>
    /// Discovers dynamic raw proxy list endpoints by scraping DuckDuckGo HTML search results.
    /// </summary>
    public class DynamicSourceDiscoverer
    {
        private static readonly HttpClient SharedClient = CreateHttpClient();

        private static readonly string[] SearchQueries = new[]
        {
            "free proxy list raw github",
            "proxyscrape api free proxies",
            "free proxy txt list 2024"
        };

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            return client;
        }

        /// <summary>
        /// Discovers public proxy list URLs using DuckDuckGo search queries.
        /// </summary>
        /// <param name="maxResults">Maximum candidate URLs to return (default 10).</param>
        /// <returns>A list of discovered proxy list URLs.</returns>
        public async Task<List<string>> DiscoverProxyListUrlsAsync(int maxResults = 10)
        {
            var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var query in SearchQueries)
            {
                if (candidates.Count >= maxResults) break;

                try
                {
                    string searchUrl = $"https://html.duckduckgo.com/html/?q={Uri.EscapeDataString(query)}";
                    string html = await SharedClient.GetStringAsync(searchUrl).ConfigureAwait(false);

                    if (!string.IsNullOrWhiteSpace(html))
                    {
                        var doc = new HtmlDocument();
                        doc.LoadHtml(html);

                        var nodes = doc.DocumentNode.SelectNodes("//a[contains(@class, 'result__url')]")
                                ?? doc.DocumentNode.SelectNodes("//a[contains(@class, 'result__a')]");

                        if (nodes != null)
                        {
                            foreach (var node in nodes)
                            {
                                string targetUrl = ExtractTargetUrl(node);
                                if (!string.IsNullOrEmpty(targetUrl) && IsCandidateProxyListUrl(targetUrl))
                                {
                                    candidates.Add(targetUrl);
                                    if (candidates.Count >= maxResults) break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"DynamicSourceDiscoverer query error: {ex.Message}");
                }
            }

            return candidates.Take(maxResults).ToList();
        }

        /// <summary>
        /// Extracts and decodes actual target URL from DuckDuckGo redirect link node.
        /// </summary>
        private string ExtractTargetUrl(HtmlNode node)
        {
            if (node == null) return string.Empty;

            string rawHref = node.GetAttributeValue("href", "").Trim();
            if (rawHref.Contains("uddg="))
            {
                int idx = rawHref.IndexOf("uddg=", StringComparison.Ordinal);
                string extracted = rawHref.Substring(idx + 5);
                int ampIdx = extracted.IndexOf('&');
                if (ampIdx >= 0)
                {
                    extracted = extracted.Substring(0, ampIdx);
                }
                string decoded = Uri.UnescapeDataString(extracted);
                if (IsValidProxyListUrl(decoded)) return decoded;
            }

            string innerText = node.InnerText.Trim();
            if (innerText.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                innerText.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                if (IsValidProxyListUrl(innerText)) return innerText;
            }

            if (IsValidProxyListUrl(rawHref)) return rawHref;

            return string.Empty;
        }

        /// <summary>
        /// Determines whether a candidate URL matches common proxy list provider patterns.
        /// </summary>
        private bool IsCandidateProxyListUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;

            string lower = url.ToLowerInvariant();

            bool isGithubTxt = lower.Contains("githubusercontent.com") && lower.EndsWith(".txt");
            bool isProxyscrape = lower.Contains("proxyscrape.com");
            bool isProxyListTxt = lower.Contains("proxy-list") && lower.EndsWith(".txt");
            bool isRawProxy = lower.Contains("raw") && lower.Contains("proxy");

            return isGithubTxt || isProxyscrape || isProxyListTxt || isRawProxy;
        }

        /// <summary>
        /// Validates if a string is a properly formatted HTTP/HTTPS URL.
        /// </summary>
        /// <param name="url">Target URL string.</param>
        /// <returns>True if URL format is valid; otherwise false.</returns>
        public static bool IsValidProxyListUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;

            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult))
            {
                return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
            }

            return false;
        }
    }
}
