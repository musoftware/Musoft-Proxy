using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProxyForge.Core
{
    /// <summary>
    /// Represents the evaluation result from a proxy judge test.
    /// </summary>
    public class JudgeResult
    {
        /// <summary>
        /// Gets or sets the detected external IP address.
        /// </summary>
        public string Ip { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the detected country.
        /// </summary>
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the anonymity level ("Elite", "Anonymous", "Transparent").
        /// </summary>
        public string AnonymityLevel { get; set; } = string.Empty;

        /// <summary>
        /// Gets whether the proxy provides Elite high-anonymity protection.
        /// </summary>
        public bool IsElite => string.Equals(AnonymityLevel, ProxyConstants.Anonymity.Elite, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Evaluates proxy anonymity levels, public IP, and header leakage.
    /// </summary>
    public class ProxyJudge
    {
        private const string DefaultJudgeUrl = ProxyConstants.DefaultJudgeUrl;

        /// <summary>
        /// Asynchronously judges a proxy's external IP, country, and anonymity level.
        /// </summary>
        /// <param name="proxy">Target proxy to judge.</param>
        /// <returns>A <see cref="JudgeResult"/> object.</returns>
        public async Task<JudgeResult> JudgeAsync(ProxyInfo proxy)
        {
            if (proxy == null) throw new ArgumentNullException(nameof(proxy));

            var result = new JudgeResult
            {
                AnonymityLevel = ProxyConstants.Anonymity.Transparent
            };

            try
            {
                using var client = ProxyFactory.CreateClient(proxy);
                client.Timeout = TimeSpan.FromSeconds(8);

                using var response = await client.GetAsync(DefaultJudgeUrl).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(content);
                            if (doc.RootElement.TryGetProperty("ip", out var ipElem))
                            {
                                result.Ip = ipElem.GetString() ?? string.Empty;
                            }
                        }
                        catch (JsonException ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"ProxyJudge JSON parse error: {ex.Message}");
                            result.Ip = content.Trim();
                        }
                    }

                    // Check response headers for proxy leakage
                    bool headerProxyLeak = false;
                    foreach (var header in response.Headers)
                    {
                        string headerName = header.Key.ToLowerInvariant();
                        if (headerName.Contains("via") ||
                            headerName.Contains("forwarded") ||
                            headerName.Contains("proxy") ||
                            headerName.Contains("client-ip"))
                        {
                            headerProxyLeak = true;
                            break;
                        }
                    }

                    if (headerProxyLeak)
                    {
                        result.AnonymityLevel = ProxyConstants.Anonymity.Transparent;
                    }
                    else
                    {
                        // High anonymity / Elite when no proxy headers are exposed
                        result.AnonymityLevel = ProxyConstants.Anonymity.Elite;
                    }

                    proxy.AnonymityLevel = result.AnonymityLevel;
                    if (!string.IsNullOrEmpty(result.Country))
                    {
                        proxy.CountryCode = result.Country;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProxyJudge error: {ex.Message}");
                result.AnonymityLevel = ProxyConstants.Anonymity.Unknown;
            }

            return result;
        }
    }
}
