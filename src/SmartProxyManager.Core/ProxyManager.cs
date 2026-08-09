using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace SmartProxyManager.Core
{
    /// <summary>
    /// High-level Proxy Manager wrapping <see cref="ProxyPool"/>, health checking, and request metrics.
    /// </summary>
    public class ProxyManager
    {
        private readonly object _lock = new object();

        /// <summary>
        /// Gets the underlying thread-safe <see cref="ProxyPool"/> engine.
        /// </summary>
        public ProxyPool Pool { get; } = new ProxyPool();

        /// <summary>
        /// Gets the proxy statistics tracker.
        /// </summary>
        public ProxyStatistics Statistics { get; } = new ProxyStatistics();

        /// <summary>
        /// Gets the list of proxies managed by this instance.
        /// </summary>
        public List<ProxyInfo> Proxies => Pool.Proxies;

        /// <summary>
        /// Gets or sets whether proxy routing is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets default proxy protocol type.
        /// </summary>
        public ProxyType DefaultType { get; set; } = ProxyType.HTTP;

        /// <summary>
        /// Gets or sets rotation mode.
        /// </summary>
        public RotationMode Rotation
        {
            get => Pool.Mode;
            set => Pool.Mode = value;
        }

        /// <summary>
        /// Gets or sets background health auto-check interval.
        /// </summary>
        public TimeSpan AutoCheckInterval { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets maximum failure threshold before placing a proxy into cooldown.
        /// </summary>
        public int MaxFailCount { get; set; } = 3;

        /// <summary>
        /// Occurs when the proxy list changes.
        /// </summary>
        public event EventHandler? ProxyListChanged;

        /// <summary>
        /// Adds a single proxy to the manager.
        /// </summary>
        public void Add(ProxyInfo proxy)
        {
            if (proxy == null) return;
            lock (_lock)
            {
                Pool.Proxies.Add(proxy);
            }
            OnProxyListChanged();
        }

        /// <summary>
        /// Adds multiple proxies to the manager.
        /// </summary>
        public void AddRange(IEnumerable<ProxyInfo> proxies)
        {
            if (proxies == null) return;
            lock (_lock)
            {
                Pool.Proxies.AddRange(proxies);
            }
            OnProxyListChanged();
        }

        /// <summary>
        /// Removes a proxy from the manager.
        /// </summary>
        public void Remove(ProxyInfo proxy)
        {
            if (proxy == null) return;
            lock (_lock)
            {
                Pool.Proxies.Remove(proxy);
            }
            OnProxyListChanged();
        }

        /// <summary>
        /// Clears all proxies from the manager.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                Pool.Proxies.Clear();
            }
            OnProxyListChanged();
        }

        /// <summary>
        /// Retrieves the next available proxy from the internal pool.
        /// </summary>
        public ProxyInfo? GetNext(string? sessionKey = null)
        {
            if (!IsEnabled) return null;
            return Pool.GetProxy(sessionKey);
        }

        /// <summary>
        /// Marks a proxy as failed and increments stats.
        /// </summary>
        public void MarkAsDead(ProxyInfo proxy)
        {
            if (proxy == null) return;
            Pool.MarkFailed(proxy);
            Statistics.RecordFailure(proxy);
        }

        /// <summary>
        /// Marks a proxy as successful.
        /// </summary>
        public void MarkAsSuccess(ProxyInfo proxy)
        {
            if (proxy == null) return;
            Pool.MarkSuccess(proxy);
            Statistics.RecordSuccess(proxy);
        }

        /// <summary>
        /// Creates an <see cref="HttpClientHandler"/> configured with the rotated or specified proxy.
        /// </summary>
        public HttpClientHandler CreateHandler(ProxyInfo? proxy = null, string? sessionKey = null)
        {
            ProxyInfo? targetProxy = proxy ?? GetNext(sessionKey);
            var handler = new HttpClientHandler();

            if (targetProxy == null || !IsEnabled)
            {
                return handler;
            }

            if (targetProxy.Type == ProxyType.SOCKS5)
            {
                var socksProxy = new MihaZupan.HttpToSocks5Proxy(
                    targetProxy.Host,
                    targetProxy.Port,
                    string.IsNullOrEmpty(targetProxy.Username) ? null : targetProxy.Username,
                    string.IsNullOrEmpty(targetProxy.Password) ? null : targetProxy.Password
                );
                handler.Proxy = socksProxy;
            }
            else
            {
                var webProxy = new WebProxy(targetProxy.Host, targetProxy.Port);
                if (!string.IsNullOrEmpty(targetProxy.Username))
                {
                    webProxy.Credentials = new NetworkCredential(targetProxy.Username, targetProxy.Password);
                }
                handler.Proxy = webProxy;
            }

            return handler;
        }

        /// <summary>
        /// Saves proxy list to JSON file.
        /// </summary>
        public void Save(string path)
        {
            lock (_lock)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(Pool.Proxies, options);
                File.WriteAllText(path, json);
            }
        }

        /// <summary>
        /// Loads proxy list from JSON file.
        /// </summary>
        public void Load(string path)
        {
            if (!File.Exists(path)) return;
            lock (_lock)
            {
                string json = File.ReadAllText(path);
                var loaded = JsonSerializer.Deserialize<List<ProxyInfo>>(json);
                if (loaded != null)
                {
                    Pool.Proxies = loaded;
                }
            }
            OnProxyListChanged();
        }

        /// <summary>
        /// Raises the <see cref="ProxyListChanged"/> event.
        /// </summary>
        protected virtual void OnProxyListChanged()
        {
            ProxyListChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
