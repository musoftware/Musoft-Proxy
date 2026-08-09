using System;
using System.Collections.Generic;
using System.Linq;

namespace ProxyForge.Core
{
    /// <summary>
    /// Event arguments supplied when a proxy rotation event is fired.
    /// </summary>
    public class ProxyRotatedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the previously active proxy.
        /// </summary>
        public ProxyInfo? OldProxy { get; }

        /// <summary>
        /// Gets the newly selected proxy.
        /// </summary>
        public ProxyInfo? NewProxy { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="ProxyRotatedEventArgs"/>.
        /// </summary>
        public ProxyRotatedEventArgs(ProxyInfo? oldProxy, ProxyInfo? newProxy)
        {
            OldProxy = oldProxy;
            NewProxy = newProxy;
        }
    }

    /// <summary>
    /// Thread-safe central pool managing proxy lists, failure cooldowns, rotation intervals, and strategy resolution.
    /// </summary>
    public class ProxyPool
    {
        private readonly object _lock = new object();
        private ProxyInfo? _currentProxy;
        private int _requestCounter = 0;
        private DateTime _lastRotationTime = DateTime.UtcNow;

        private List<ProxyInfo> _proxies = new List<ProxyInfo>();

        /// <summary>
        /// Gets or sets the list of proxies managed by this pool.
        /// </summary>
        public List<ProxyInfo> Proxies
        {
            get
            {
                lock (_lock)
                {
                    return _proxies;
                }
            }
            set
            {
                lock (_lock)
                {
                    _proxies = value ?? new List<ProxyInfo>();
                }
            }
        }

        /// <summary>
        /// Gets or sets the rotation strategy algorithm.
        /// </summary>
        public IRotationStrategy Strategy { get; set; } = new RoundRobinStrategy();

        /// <summary>
        /// Gets or sets the proxy rotation trigger mode.
        /// </summary>
        public RotationMode Mode { get; set; } = RotationMode.EveryRequest;

        /// <summary>
        /// Gets or sets the request threshold when <see cref="Mode"/> is set to <see cref="RotationMode.EveryNRequests"/>.
        /// </summary>
        public int RotateAfter { get; set; } = 10;

        /// <summary>
        /// Gets or sets the time threshold in seconds when <see cref="Mode"/> is set to <see cref="RotationMode.EveryNSeconds"/>.
        /// </summary>
        public int RotateAfterSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the duration of sticky sessions.
        /// </summary>
        public TimeSpan StickyDuration { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Gets or sets maximum failure threshold before placing a proxy into cooldown.
        /// </summary>
        public int MaxFailCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the cooldown duration applied when a proxy fails or is banned.
        /// </summary>
        public TimeSpan CooldownDuration { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Occurs whenever a proxy rotation is triggered.
        /// </summary>
        public event EventHandler<ProxyRotatedEventArgs>? OnProxyRotated;

        /// <summary>
        /// Gets a proxy from the pool according to the active mode and strategy.
        /// </summary>
        /// <param name="sessionKey">Optional session key for sticky session binding.</param>
        /// <returns>A selected <see cref="ProxyInfo"/> or null if pool is empty.</returns>
        public ProxyInfo? GetProxy(string? sessionKey = null)
        {
            ProxyRotatedEventArgs? eventToRaise = null;
            ProxyInfo? selectedResult = null;

            lock (_lock)
            {
                var available = Proxies.Where(p => !p.IsBanned && !p.IsInCooldown && p.IsLive != false).ToList();
                if (available.Count == 0)
                {
                    // Fallback to non-banned proxies
                    available = Proxies.Where(p => !p.IsBanned).ToList();
                }

                if (available.Count == 0) return null;

                bool shouldRotate = false;

                if (_currentProxy == null || !available.Contains(_currentProxy))
                {
                    shouldRotate = true;
                }
                else
                {
                    switch (Mode)
                    {
                        case RotationMode.EveryRequest:
                        case RotationMode.RoundRobin:
                        case RotationMode.Random:
                            shouldRotate = true;
                            break;

                        case RotationMode.EveryNRequests:
                            _requestCounter++;
                            if (_requestCounter >= RotateAfter)
                            {
                                _requestCounter = 0;
                                shouldRotate = true;
                            }
                            break;

                        case RotationMode.EveryNSeconds:
                            if ((DateTime.UtcNow - _lastRotationTime).TotalSeconds >= RotateAfterSeconds)
                            {
                                _lastRotationTime = DateTime.UtcNow;
                                shouldRotate = true;
                            }
                            break;

                        case RotationMode.Sticky:
                            if (Strategy is StickySessionStrategy stickyStrategy)
                            {
                                var stickyProxy = stickyStrategy.SelectProxy(available, _currentProxy, sessionKey ?? "default");
                                if (stickyProxy != _currentProxy)
                                {
                                    shouldRotate = true;
                                }
                                else
                                {
                                    stickyProxy.LastUsed = DateTime.UtcNow;
                                    return stickyProxy;
                                }
                            }
                            break;
                    }
                }

                if (shouldRotate)
                {
                    ProxyInfo? oldProxy = _currentProxy;
                    ProxyInfo? newProxy = Strategy.SelectProxy(available, _currentProxy);

                    if (newProxy != null)
                    {
                        newProxy.LastUsed = DateTime.UtcNow;
                        _currentProxy = newProxy;

                        if (oldProxy != newProxy)
                        {
                            eventToRaise = new ProxyRotatedEventArgs(oldProxy, newProxy);
                        }
                    }

                    selectedResult = newProxy;
                }
                else
                {
                    if (_currentProxy != null)
                    {
                        _currentProxy.LastUsed = DateTime.UtcNow;
                    }

                    selectedResult = _currentProxy;
                }
            }

            if (eventToRaise != null)
            {
                OnProxyRotated?.Invoke(this, eventToRaise);
            }

            return selectedResult;
        }

        /// <summary>
        /// Gets or sets the ban detector instance used for inspecting HTTP responses.
        /// </summary>
        public BanDetector Detector { get; set; } = new BanDetector();

        /// <summary>
        /// Marks a proxy request as failed, incrementing its fail count and placing it into cooldown.
        /// </summary>
        /// <param name="proxy">Target proxy.</param>
        public void MarkFailed(ProxyInfo proxy)
        {
            if (proxy == null) return;
            lock (_lock)
            {
                proxy.FailCount++;
                if (proxy.IsBanned || proxy.FailCount >= MaxFailCount)
                {
                    proxy.CooldownUntil = DateTime.UtcNow.Add(CooldownDuration);
                    proxy.IsLive = false;
                }
            }
        }

        /// <summary>
        /// Inspects an HTTP response and body for ban/captcha indicators, automatically marking the proxy as banned and putting it into cooldown if detected.
        /// </summary>
        /// <param name="response">The HTTP response message.</param>
        /// <param name="body">The response body string.</param>
        /// <param name="proxyUsed">The proxy that was used for the request.</param>
        public void CheckIfBanned(System.Net.Http.HttpResponseMessage? response, string? body, ProxyInfo? proxyUsed)
        {
            if (proxyUsed == null) return;
            if (Detector.IsBanned(response, body))
            {
                lock (_lock)
                {
                    proxyUsed.IsBanned = true;
                    proxyUsed.CooldownUntil = DateTime.UtcNow.Add(CooldownDuration);
                    proxyUsed.IsLive = false;
                }
            }
        }

        /// <summary>
        /// Marks a proxy request as successful, resetting its failure counter.
        /// </summary>
        /// <param name="proxy">Target proxy.</param>
        public void MarkSuccess(ProxyInfo proxy)
        {
            if (proxy == null) return;
            lock (_lock)
            {
                proxy.FailCount = 0;
                proxy.IsLive = true;
            }
        }
    }
}
