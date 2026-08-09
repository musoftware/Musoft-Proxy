using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ProxyForge.Core
{
    /// <summary>
    /// Implements a sticky session strategy that binds a specific proxy to a session key for a configurable duration.
    /// </summary>
    public class StickySessionStrategy : IRotationStrategy
    {
        private class StickyEntry
        {
            public ProxyInfo Proxy { get; set; } = null!;
            public DateTime ExpiresAt { get; set; }
        }

        private readonly ConcurrentDictionary<string, StickyEntry> _sessions = new ConcurrentDictionary<string, StickyEntry>();
        private readonly IRotationStrategy _fallbackStrategy;

        /// <summary>
        /// Gets or sets the duration for which a proxy remains sticky to a session key.
        /// </summary>
        public TimeSpan StickyDuration { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Initializes a new instance of <see cref="StickySessionStrategy"/>.
        /// </summary>
        /// <param name="stickyDuration">Default sticky session expiration duration.</param>
        /// <param name="fallbackStrategy">Strategy used to select a new proxy when sticky session expires or starts.</param>
        public StickySessionStrategy(TimeSpan? stickyDuration = null, IRotationStrategy? fallbackStrategy = null)
        {
            if (stickyDuration.HasValue) StickyDuration = stickyDuration.Value;
            _fallbackStrategy = fallbackStrategy ?? new RoundRobinStrategy();
        }

        /// <summary>
        /// Selects a proxy bound to a session key or assigns a new sticky proxy.
        /// </summary>
        /// <param name="availableProxies">List of available proxies.</param>
        /// <param name="currentProxy">Current proxy context.</param>
        /// <returns>A sticky <see cref="ProxyInfo"/> or null.</returns>
        public ProxyInfo? SelectProxy(IReadOnlyList<ProxyInfo> availableProxies, ProxyInfo? currentProxy)
        {
            return SelectProxy(availableProxies, currentProxy, sessionKey: "default");
        }

        /// <summary>
        /// Selects a sticky proxy for a specific session key.
        /// </summary>
        /// <param name="availableProxies">Available proxies.</param>
        /// <param name="currentProxy">Current proxy context.</param>
        /// <param name="sessionKey">Unique session identifier.</param>
        /// <returns>The assigned sticky proxy.</returns>
        public ProxyInfo? SelectProxy(IReadOnlyList<ProxyInfo> availableProxies, ProxyInfo? currentProxy, string sessionKey)
        {
            if (availableProxies == null || availableProxies.Count == 0) return null;

            string key = string.IsNullOrEmpty(sessionKey) ? "default" : sessionKey;

            if (_sessions.TryGetValue(key, out var entry))
            {
                if (DateTime.UtcNow < entry.ExpiresAt && availableProxies.Any(p => p.Equals(entry.Proxy)) && !entry.Proxy.IsInCooldown && !entry.Proxy.IsBanned)
                {
                    return entry.Proxy;
                }
            }

            ProxyInfo? newProxy = _fallbackStrategy.SelectProxy(availableProxies, currentProxy);
            if (newProxy != null)
            {
                _sessions[key] = new StickyEntry
                {
                    Proxy = newProxy,
                    ExpiresAt = DateTime.UtcNow.Add(StickyDuration)
                };
            }

            return newProxy;
        }
    }
}
