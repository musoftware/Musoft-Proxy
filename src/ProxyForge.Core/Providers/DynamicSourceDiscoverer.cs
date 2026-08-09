using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ProxyForge.Core
{
    /// <summary>
    /// Discovers dynamic raw proxy list endpoints by querying search engines, GitHub APIs, and curated repository patterns.
    /// </summary>
    public class DynamicSourceDiscoverer
    {
        private static readonly HttpClient SharedClient = CreateHttpClient();

        private static readonly string[] CuratedFallbackSources = new[]
        {
            "https://api.proxyscrape.com/v4/free-proxy-list/get?request=display_proxies&protocol=http&country=all&format=text",
            "https://api.proxyscrape.com/v4/free-proxy-list/get?request=display_proxies&protocol=socks5&country=all&format=text",
            "https://api.proxyscrape.com/v2/?request=getproxies&protocol=http&timeout=10000&country=all",
            "https://api.proxyscrape.com/v2/?request=getproxies&protocol=socks5&timeout=10000&country=all",
            "https://raw.githubusercontent.com/monosans/proxy-list/main/proxies/http.txt",
            "https://raw.githubusercontent.com/monosans/proxy-list/main/proxies/socks5.txt",
            "https://raw.githubusercontent.com/monosans/proxy-list/main/proxies_anonymous/http.txt",
            "https://raw.githubusercontent.com/monosans/proxy-list/main/proxies_anonymous/socks5.txt",
            "https://raw.githubusercontent.com/clarketm/proxy-list/master/proxy-list-raw.txt",
            "https://raw.githubusercontent.com/sunny9577/proxy-scraper/master/generated/http_proxies.txt",
            "https://raw.githubusercontent.com/sunny9577/proxy-scraper/master/generated/socks5_proxies.txt",
            "https://raw.githubusercontent.com/mmpx12/proxy-list/master/http.txt",
            "https://raw.githubusercontent.com/mmpx12/proxy-list/master/socks5.txt",
            "https://raw.githubusercontent.com/roosterkid/openproxylist/main/HTTPS_RAW.txt",
            "https://raw.githubusercontent.com/roosterkid/openproxylist/main/SOCKS5_RAW.txt",
            "https://raw.githubusercontent.com/officialputuid/KangProxy/KangProxy/http/http.txt",
            "https://raw.githubusercontent.com/officialputuid/KangProxy/KangProxy/socks5/socks5.txt",
            "https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/http.txt",
            "https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks5.txt",
            "https://raw.githubusercontent.com/TheSpeedX/SOCKS-List/master/http.txt",
            "https://raw.githubusercontent.com/TheSpeedX/SOCKS-List/master/socks5.txt",
            "https://raw.githubusercontent.com/ShiftyTR/Proxy-List/master/http.txt",
            "https://raw.githubusercontent.com/ShiftyTR/Proxy-List/master/socks5.txt",
            "https://raw.githubusercontent.com/jetkai/proxy-list/main/online-proxies/txt/proxies-http.txt",
            "https://raw.githubusercontent.com/jetkai/proxy-list/main/online-proxies/txt/proxies-socks5.txt",
            "https://raw.githubusercontent.com/ErcinDedeoglu/proxies/main/proxies/http.txt",
            "https://raw.githubusercontent.com/ErcinDedeoglu/proxies/main/proxies/socks5.txt",
            "https://raw.githubusercontent.com/HyperBeats/proxy-list/main/http.txt",
            "https://raw.githubusercontent.com/HyperBeats/proxy-list/main/socks5.txt",
            "https://raw.githubusercontent.com/hookzof/socks5_list/master/proxy.txt",
            "https://raw.githubusercontent.com/Anonym0usWork1221/Free-Proxies/main/proxy_files/http_proxies.txt",
            "https://raw.githubusercontent.com/Anonym0usWork1221/Free-Proxies/main/proxy_files/socks5_proxies.txt",
            "https://raw.githubusercontent.com/zloi-user/hideip.me/main/http.txt",
            "https://raw.githubusercontent.com/zloi-user/hideip.me/main/socks5.txt",
            "https://raw.githubusercontent.com/proxifly/free-proxy-list/main/proxies/all/data.txt",
            "https://raw.githubusercontent.com/rdavydov/proxy-list/main/http.txt",
            "https://raw.githubusercontent.com/rdavydov/proxy-list/main/socks5.txt",
            "https://raw.githubusercontent.com/prxchk/proxy-list/main/http.txt",
            "https://raw.githubusercontent.com/prxchk/proxy-list/main/socks5.txt",
            "https://openproxylist.xyz/http.txt",
            "https://openproxylist.xyz/socks5.txt"
        };

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            return client;
        }

        /// <summary>
        /// Discovers public proxy list URLs using DuckDuckGo search, GitHub API, and curated repository patterns.
        /// </summary>
        /// <param name="maxResults">Maximum candidate URLs to return (default 30).</param>
        /// <returns>A list of discovered proxy list URLs.</returns>
        public async Task<List<string>> DiscoverProxyListUrlsAsync(int maxResults = 30)
        {
            var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Discover via GitHub API search
            try
            {
                await DiscoverFromGitHubApiAsync(candidates, maxResults).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GitHub API discovery error: {ex.Message}");
            }

            // 2. Discover via DuckDuckGo search engine
            if (candidates.Count < maxResults)
            {
                try
                {
                    await DiscoverFromDuckDuckGoAsync(candidates, maxResults).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"DuckDuckGo discovery error: {ex.Message}");
                }
            }

            // 3. Fallback to curated pool of active proxy list endpoints
            foreach (var url in CuratedFallbackSources)
            {
                if (candidates.Count >= maxResults) break;
                candidates.Add(url);
            }

            return candidates.Take(maxResults).ToList();
        }

        private async Task DiscoverFromGitHubApiAsync(HashSet<string> candidates, int maxResults)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/search/repositories?q=proxy+list&sort=updated&per_page=10");
            req.Headers.UserAgent.ParseAdd("ProxyForge/1.0");

            using var resp = await SharedClient.SendAsync(req).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode) return;

            string json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in items.EnumerateArray())
                {
                    if (candidates.Count >= maxResults) break;

                    if (item.TryGetProperty("full_name", out var fullNameProp))
                    {
                        string fullName = fullNameProp.GetString() ?? "";
                        if (!string.IsNullOrEmpty(fullName))
                        {
                            AddGitHubRawCandidates(fullName, candidates, maxResults);
                        }
                    }
                }
            }
        }

        private async Task DiscoverFromDuckDuckGoAsync(HashSet<string> candidates, int maxResults)
        {
            string[] searchQueries = new[]
            {
                "free proxy list raw github",
                "proxyscrape api free proxies",
                "free proxy list github http socks5"
            };

            foreach (var query in searchQueries)
            {
                if (candidates.Count >= maxResults) break;

                string searchUrl = $"https://html.duckduckgo.com/html/?q={Uri.EscapeDataString(query)}";
                using var req = new HttpRequestMessage(HttpMethod.Get, searchUrl);
                using var resp = await SharedClient.SendAsync(req).ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode) continue;

                string html = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(html)) continue;

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var nodes = doc.DocumentNode.SelectNodes("//a[@href]");
                if (nodes == null) continue;

                foreach (var node in nodes)
                {
                    if (candidates.Count >= maxResults) break;

                    string targetUrl = ExtractTargetUrl(node);
                    if (string.IsNullOrEmpty(targetUrl)) continue;

                    if (IsCandidateProxyListUrl(targetUrl))
                    {
                        candidates.Add(targetUrl);
                    }
                    else if (targetUrl.Contains("github.com/"))
                    {
                        var match = Regex.Match(targetUrl, @"github\.com/([^/]+)/([^/]+)", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            string owner = match.Groups[1].Value;
                            string repo = match.Groups[2].Value.Replace(".git", "");
                            AddGitHubRawCandidates($"{owner}/{repo}", candidates, maxResults);
                        }
                    }
                }
            }
        }

        private static void AddGitHubRawCandidates(string repoFullName, HashSet<string> candidates, int maxResults)
        {
            string[] paths = new[]
            {
                "main/proxies/http.txt",
                "main/proxies/socks5.txt",
                "main/http.txt",
                "main/socks5.txt",
                "main/proxy.txt",
                "master/http.txt",
                "master/socks5.txt",
                "master/proxy-list-raw.txt",
                "master/proxy.txt"
            };

            foreach (var p in paths)
            {
                if (candidates.Count >= maxResults) break;
                candidates.Add($"https://raw.githubusercontent.com/{repoFullName}/{p}");
            }
        }

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

        private bool IsCandidateProxyListUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;

            string lower = url.ToLowerInvariant();

            bool isGithubRaw = lower.Contains("raw.githubusercontent.com");
            bool isProxyscrape = lower.Contains("proxyscrape.com");
            bool isProxyListTxt = lower.Contains("proxy") && (lower.EndsWith(".txt") || lower.Contains("raw"));

            return isGithubRaw || isProxyscrape || isProxyListTxt;
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
