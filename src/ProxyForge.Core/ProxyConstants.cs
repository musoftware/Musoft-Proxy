using System;

namespace ProxyForge.Core
{
    /// <summary>
    /// Centralized constants and configuration default values for ProxyForge.
    /// </summary>
    public static class ProxyConstants
    {
        /// <summary>
        /// Default remote IP test endpoint URL.
        /// </summary>
        public const string DefaultTestUrl = "https://api.ipify.org";

        /// <summary>
        /// Default proxy judge evaluation URL endpoint.
        /// </summary>
        public const string DefaultJudgeUrl = "https://api.ipify.org?format=json";

        /// <summary>
        /// Standard User-Agent header used for HTTP proxy scraping and source discovery.
        /// </summary>
        public const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

        /// <summary>
        /// Default timeout in milliseconds for proxy testing (7000ms).
        /// </summary>
        public const int DefaultTestTimeoutMs = 7000;

        /// <summary>
        /// Default maximum consecutive failure threshold before placing a proxy into cooldown.
        /// </summary>
        public const int DefaultMaxFailCount = 3;

        /// <summary>
        /// Default cooldown duration for failed or banned proxies (10 minutes).
        /// </summary>
        public static readonly TimeSpan DefaultCooldownDuration = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Default JSON persistence file path.
        /// </summary>
        public const string DefaultJsonFileName = "proxies.json";

        /// <summary>
        /// Anonymity levels.
        /// </summary>
        public static class Anonymity
        {
            public const string Elite = "Elite";
            public const string Anonymous = "Anonymous";
            public const string Transparent = "Transparent";
            public const string Unknown = "Unknown";
        }
    }
}
