using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ProxyForge.Core
{
    /// <summary>
    /// Growth provider feature that scrapes free proxies from public online endpoints with dynamic self-healing source discovery.
    /// </summary>
    public class FreeProxyScraper : IProxyProvider
    {
        private static readonly HttpClient SharedHttpClient = CreateHttpClient();

        private static readonly ConcurrentDictionary<string, ProxyType> _sources = new ConcurrentDictionary<string, ProxyType>();

        /// <summary>
        /// Gets or sets the time interval between automatic dynamic source discovery runs.
        /// </summary>
        public TimeSpan SourceDiscoveryInterval { get; set; } = TimeSpan.FromHours(6);

        private DateTime _lastDiscovery = DateTime.MinValue;

        static FreeProxyScraper()
        {
            InitializeDefaultSources();
        }

        private static void InitializeDefaultSources()
        {
            // ── ProxyScrape v4 & v2 APIs ──────────────────────────────────────────
            _sources.TryAdd("https://api.proxyscrape.com/v4/free-proxy-list/get?request=display_proxies&protocol=http&country=all&format=text", ProxyType.HTTP);
            _sources.TryAdd("https://api.proxyscrape.com/v4/free-proxy-list/get?request=display_proxies&protocol=socks5&country=all&format=text", ProxyType.SOCKS5);
            _sources.TryAdd("https://api.proxyscrape.com/v2/?request=getproxies&protocol=http&timeout=10000&country=all", ProxyType.HTTP);
            _sources.TryAdd("https://api.proxyscrape.com/v2/?request=getproxies&protocol=socks5&timeout=10000&country=all", ProxyType.SOCKS5);

            // ── Curated High-Yield GitHub Raw Repositories ───────────────────────
            _sources.TryAdd("https://raw.githubusercontent.com/monosans/proxy-list/main/proxies/http.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/monosans/proxy-list/main/proxies/socks5.txt", ProxyType.SOCKS5);
            _sources.TryAdd("https://raw.githubusercontent.com/monosans/proxy-list/main/proxies_anonymous/http.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/monosans/proxy-list/main/proxies_anonymous/socks5.txt", ProxyType.SOCKS5);
            _sources.TryAdd("https://raw.githubusercontent.com/clarketm/proxy-list/master/proxy-list-raw.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/sunny9577/proxy-scraper/master/generated/http_proxies.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/sunny9577/proxy-scraper/master/generated/socks5_proxies.txt", ProxyType.SOCKS5);
            _sources.TryAdd("https://raw.githubusercontent.com/mmpx12/proxy-list/master/http.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/mmpx12/proxy-list/master/socks5.txt", ProxyType.SOCKS5);
            _sources.TryAdd("https://raw.githubusercontent.com/roosterkid/openproxylist/main/HTTPS_RAW.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/roosterkid/openproxylist/main/SOCKS5_RAW.txt", ProxyType.SOCKS5);
            _sources.TryAdd("https://raw.githubusercontent.com/officialputuid/KangProxy/KangProxy/http/http.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/officialputuid/KangProxy/KangProxy/socks5/socks5.txt", ProxyType.SOCKS5);
            _sources.TryAdd("https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/http.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks5.txt", ProxyType.SOCKS5);
            _sources.TryAdd("https://raw.githubusercontent.com/TheSpeedX/SOCKS-List/master/http.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/TheSpeedX/SOCKS-List/master/socks5.txt", ProxyType.SOCKS5);
            _sources.TryAdd("https://raw.githubusercontent.com/ShiftyTR/Proxy-List/master/http.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/ShiftyTR/Proxy-List/master/socks5.txt", ProxyType.SOCKS5);
            _sources.TryAdd("https://raw.githubusercontent.com/jetkai/proxy-list/main/online-proxies/txt/proxies-http.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/jetkai/proxy-list/main/online-proxies/txt/proxies-socks5.txt", ProxyType.SOCKS5);
            _sources.TryAdd("https://raw.githubusercontent.com/ErcinDedeoglu/proxies/main/proxies/http.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/ErcinDedeoglu/proxies/main/proxies/socks5.txt", ProxyType.SOCKS5);
            _sources.TryAdd("https://raw.githubusercontent.com/HyperBeats/proxy-list/main/http.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/HyperBeats/proxy-list/main/socks5.txt", ProxyType.SOCKS5);
            _sources.TryAdd("https://raw.githubusercontent.com/hookzof/socks5_list/master/proxy.txt", ProxyType.SOCKS5);
            _sources.TryAdd("https://raw.githubusercontent.com/Anonym0usWork1221/Free-Proxies/main/proxy_files/http_proxies.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/Anonym0usWork1221/Free-Proxies/main/proxy_files/socks5_proxies.txt", ProxyType.SOCKS5);
            _sources.TryAdd("https://raw.githubusercontent.com/zloi-user/hideip.me/main/http.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/zloi-user/hideip.me/main/socks5.txt", ProxyType.SOCKS5);
            _sources.TryAdd("https://raw.githubusercontent.com/proxifly/free-proxy-list/main/proxies/all/data.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/rdavydov/proxy-list/main/http.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/rdavydov/proxy-list/main/socks5.txt", ProxyType.SOCKS5);
            _sources.TryAdd("https://raw.githubusercontent.com/prxchk/proxy-list/main/http.txt", ProxyType.HTTP);
            _sources.TryAdd("https://raw.githubusercontent.com/prxchk/proxy-list/main/socks5.txt", ProxyType.SOCKS5);
            _sources.TryAdd("https://openproxylist.xyz/http.txt", ProxyType.HTTP);
            _sources.TryAdd("https://openproxylist.xyz/socks5.txt", ProxyType.SOCKS5);
        }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            return client;
        }

        /// <summary>
        /// Adds a custom proxy list URL source to the active sources collection at runtime.
        /// </summary>
        /// <param name="url">Proxy list endpoint URL.</param>
        /// <param name="type">Expected proxy type protocol.</param>
        public void AddSource(string url, ProxyType type)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            _sources.TryAdd(url, type);
        }

        /// <summary>
        /// Removes a proxy list URL source from the active sources collection.
        /// </summary>
        /// <param name="url">Proxy list endpoint URL.</param>
        /// <returns>True if removed successfully; otherwise false.</returns>
        public bool RemoveSource(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            return _sources.TryRemove(url, out _);
        }

        /// <summary>
        /// Clears all proxy list sources.
        /// </summary>
        public void ClearSources()
        {
            _sources.Clear();
        }

        /// <summary>
        /// Gets a snapshot dictionary of all currently active proxy list sources.
        /// </summary>
        /// <returns>Read-only dictionary of active sources and their associated proxy types.</returns>
        public IReadOnlyDictionary<string, ProxyType> GetAllSources()
        {
            return new Dictionary<string, ProxyType>(_sources);
        }

        /// <summary>
        /// Saves active proxy sources to a JSON configuration file.
        /// </summary>
        /// <param name="path">Target JSON file path (default proxysources.json).</param>
        public void SaveSources(string path = "proxysources.json")
        {
            var dict = _sources.ToDictionary(k => k.Key, v => v.Value.ToString());
            string json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Loads active proxy sources from a JSON configuration file.
        /// </summary>
        /// <param name="path">Target JSON file path (default proxysources.json).</param>
        public void LoadSources(string path = "proxysources.json")
        {
            if (!File.Exists(path)) return;
            string json = File.ReadAllText(path);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (dict != null)
            {
                _sources.Clear();
                foreach (var kvp in dict)
                {
#if NETFRAMEWORK || NETSTANDARD2_0
                    if (Enum.IsDefined(typeof(ProxyType), kvp.Value))
                    {
                        var type = (ProxyType)Enum.Parse(typeof(ProxyType), kvp.Value, true);
                        _sources.TryAdd(kvp.Key, type);
                    }
#else
                    if (Enum.TryParse<ProxyType>(kvp.Value, true, out var type))
                    {
                        _sources.TryAdd(kvp.Key, type);
                    }
#endif
                }
            }
        }

        /// <summary>
        /// Discovers new proxy list URLs using DuckDuckGo search queries and adds valid ones to active sources.
        /// </summary>
        /// <returns>Number of newly discovered and added proxy list sources.</returns>
        public async Task<int> DiscoverAndAddNewSourcesAsync()
        {
            var discoverer = new DynamicSourceDiscoverer();
            var urls = await discoverer.DiscoverProxyListUrlsAsync().ConfigureAwait(false);
            int added = 0;

            foreach (var url in urls)
            {
                if (!_sources.ContainsKey(url))
                {
                    var type = url.IndexOf("socks5", StringComparison.OrdinalIgnoreCase) >= 0 ? ProxyType.SOCKS5 : ProxyType.HTTP;
                    if (await IsValidProxyListUrlAsync(url).ConfigureAwait(false))
                    {
                        AddSource(url, type);
                        added++;
                    }
                }
            }

            return added;
        }

        /// <summary>
        /// Asynchronously fetches and parses proxies from public free proxy sources in parallel.
        /// Automatically performs self-healing source discovery if sources or results are low.
        /// </summary>
        /// <returns>A deduplicated list of valid <see cref="ProxyInfo"/> instances (up to 500 max).</returns>
        public async Task<List<ProxyInfo>> FetchAsync()
        {
            // Self-healing: if sources are depleted, discover new ones synchronously first
            if (_sources.Count < 3)
            {
                await DiscoverAndAddNewSourcesAsync().ConfigureAwait(false);
            }

            // Periodic auto-refresh of sources in background without blocking current fetch
            if (DateTime.UtcNow - _lastDiscovery > SourceDiscoveryInterval)
            {
                _lastDiscovery = DateTime.UtcNow;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await DiscoverAndAddNewSourcesAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Background source discovery error: {ex.Message}");
                    }
                });
            }

            var bag = new ConcurrentBag<ProxyInfo>();
            var currentSources = _sources.ToArray();

            // Parallel fetching from configured API and raw list sources
            var tasks = currentSources.Select(async kvp =>
            {
                try
                {
                    string content = await SharedHttpClient.GetStringAsync(kvp.Key).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var parsed = ProxyParser.Parse(content, kvp.Value);
                        foreach (var p in parsed)
                        {
                            if (IsValid(p))
                            {
                                bag.Add(p);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error fetching source {kvp.Key}: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);

            // Attempt HTML scraping from free-proxy-list.net
            try
            {
                string html = await SharedHttpClient.GetStringAsync("https://free-proxy-list.net/").ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(html))
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var rows = doc.DocumentNode.SelectNodes("//table[@id='proxylisttable']//tr")
                            ?? doc.DocumentNode.SelectNodes("//table[contains(@class, 'table')]//tr");

                    if (rows != null)
                    {
                        int addedCount = 0;
                        foreach (var row in rows)
                        {
                            if (addedCount >= 50) break;

                            var cols = row.SelectNodes("td");
                            if (cols != null && cols.Count >= 3)
                            {
                                string ip = cols[0].InnerText.Trim();
                                string portStr = cols[1].InnerText.Trim();
                                string country = cols[2].InnerText.Trim();

                                if (int.TryParse(portStr, out int port))
                                {
                                    var proxy = new ProxyInfo(ip, port, ProxyType.HTTP)
                                    {
                                        CountryCode = country
                                    };

                                    if (IsValid(proxy))
                                    {
                                        bag.Add(proxy);
                                        addedCount++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTML scraping error: {ex.Message}");
            }

            var results = bag
                .Where(IsValid)
                .GroupBy(p => $"{p.Type}:{p.Host}:{p.Port}")
                .Select(g => g.First())
                .Take(500)
                .ToList();

            // Self-healing: if fetched results are low (< 20), trigger background discovery for future runs
            if (results.Count < 20)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await DiscoverAndAddNewSourcesAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Low results discovery error: {ex.Message}");
                    }
                });
            }

            return results;
        }

        /// <summary>
        /// Alias method for <see cref="FetchAsync"/>.
        /// </summary>
        /// <returns>A deduplicated list of valid <see cref="ProxyInfo"/> instances.</returns>
        public async Task<List<ProxyInfo>> ScrapeAsync()
        {
            return await FetchAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Validates host, port, and non-zero IP criteria for a proxy.
        /// </summary>
        private bool IsValid(ProxyInfo? p)
        {
            if (p == null) return false;
            if (string.IsNullOrWhiteSpace(p.Host)) return false;
            if (!p.Host.Contains(".")) return false;
            if (string.Equals(p.Host.Trim(), "0.0.0.0", StringComparison.Ordinal)) return false;
            if (p.Port < 1 || p.Port > 65535) return false;
            return true;
        }

        /// <summary>
        /// Validates candidate proxy list endpoint by fetching 2KB preview and matching IP:Port regex.
        /// </summary>
        private async Task<bool> IsValidProxyListUrlAsync(string url)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                using var resp = await SharedHttpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode) return false;

                using var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
                byte[] buffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                string sample = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

                return ProxyParser.Parse(sample).Count > 0 || Regex.IsMatch(sample, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}[:\s]\d{2,5}");
            }
            catch
            {
                return false;
            }
        }
    }
}
