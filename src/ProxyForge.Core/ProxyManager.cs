using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProxyForge.Core
{
    /// <summary>
    /// High-level Proxy Manager wrapping <see cref="ProxyPool"/>, health checking, request metrics, and pluggable storage persistence.
    /// </summary>
    public class ProxyManager
    {
        private readonly object _lock = new object();

        private static IProxyStorage _storageProvider = new InMemoryStorage();

        /// <summary>
        /// Gets the active global storage provider. Defaults to <see cref="InMemoryStorage"/>.
        /// </summary>
        public static IProxyStorage StorageProvider
        {
            get => _storageProvider ??= new InMemoryStorage();
            private set => _storageProvider = value ?? new InMemoryStorage();
        }

        /// <summary>
        /// Gets the global default <see cref="ProxyManager"/> singleton instance.
        /// </summary>
        public static ProxyManager Default { get; } = new ProxyManager();

        /// <summary>
        /// Occurs when the global storage provider configuration changes.
        /// </summary>
        public static event EventHandler? GlobalStorageChanged;

        /// <summary>
        /// Configures the global storage method for ProxyManager.
        /// </summary>
        /// <param name="method">Storage method enum (InMemory or JsonFile).</param>
        /// <param name="jsonFilePath">Optional JSON file path when using JsonFile storage method.</param>
        public static void SetStorageMethod(StorageMethod method, string? jsonFilePath = null)
        {
            switch (method)
            {
                case StorageMethod.InMemory:
                    SetStorageMethod(new InMemoryStorage());
                    break;
                case StorageMethod.JsonFile:
                    SetStorageMethod(new JsonFileStorage(jsonFilePath ?? "proxies.json"));
                    break;
                case StorageMethod.CustomApi:
                    throw new ArgumentException("For CustomApi storage method, please provide an IProxyStorage implementation or custom load/save delegates.", nameof(method));
            }
        }

        /// <summary>
        /// Configures a custom global <see cref="IProxyStorage"/> provider.
        /// </summary>
        public static void SetStorageMethod(IProxyStorage storageProvider)
        {
            StorageProvider = storageProvider ?? new InMemoryStorage();
            GlobalStorageChanged?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Configures custom delegate load and save callbacks for CustomApi storage.
        /// </summary>
        public static void SetStorageMethod(Func<List<ProxyInfo>> loadFunc, Action<IEnumerable<ProxyInfo>> saveAction)
        {
            SetStorageMethod(new DelegateStorage(loadFunc, saveAction));
        }

        /// <summary>
        /// Configures custom async delegate load and save callbacks for CustomApi storage.
        /// </summary>
        public static void SetStorageMethod(Func<Task<List<ProxyInfo>>> loadAsyncFunc, Func<IEnumerable<ProxyInfo>, Task> saveAsyncFunc)
        {
            SetStorageMethod(new DelegateStorage(loadAsyncFunc, saveAsyncFunc));
        }

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
        /// Gets the currently selected active proxy.
        /// </summary>
        public ProxyInfo? CurrentProxy => Pool.CurrentProxy;

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
        /// Gets or sets whether direct connection is included in pool rotation and allowed as fallback.
        /// </summary>
        public bool AllowDirectFallback
        {
            get => Pool.AllowDirectFallback;
            set => Pool.AllowDirectFallback = value;
        }

        /// <summary>
        /// Gets or sets background health auto-check interval.
        /// </summary>
        public TimeSpan AutoCheckInterval { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets maximum failure threshold before placing a proxy into cooldown.
        /// </summary>
        public int MaxFailCount
        {
            get => Pool.MaxFailCount;
            set => Pool.MaxFailCount = value;
        }

        /// <summary>
        /// Gets or sets whether proxy operations automatically save changes to storage.
        /// </summary>
        public bool AutoSaveOnListChange { get; set; } = true;

        /// <summary>
        /// Gets or sets whether proxies evaluated as dead (IsLive == false) are automatically removed during health checks.
        /// </summary>
        public bool AutoRemoveDeadOnTest { get; set; } = true;

        private readonly IProxyStorage? _instanceStorage;

        /// <summary>
        /// Gets the active storage provider for this manager instance.
        /// </summary>
        public IProxyStorage ActiveStorage => _instanceStorage ?? StorageProvider;

        /// <summary>
        /// Occurs when the proxy list changes.
        /// </summary>
        public event EventHandler? ProxyListChanged;

        /// <summary>
        /// Initializes a new instance of <see cref="ProxyManager"/> with optional custom instance storage provider.
        /// </summary>
        /// <param name="storage">Optional custom storage provider for this instance.</param>
        public ProxyManager(IProxyStorage? storage = null)
        {
            _instanceStorage = storage;
            GlobalStorageChanged += (s, e) =>
            {
                if (_instanceStorage == null) LoadFromStorage();
            };
            LoadFromStorage();
        }

        /// <summary>
        /// Loads proxies from the active storage provider into this manager's pool.
        /// </summary>
        public void LoadFromStorage()
        {
            lock (_lock)
            {
                var data = ActiveStorage.LoadData();
                if (data != null)
                {
                    IsEnabled = data.IsEnabled;
                    DefaultType = data.DefaultType;
                    Rotation = data.RotationMode;
                    Pool.RotateAfter = data.RotateAfter > 0 ? data.RotateAfter : 10;
                    Pool.AllowDirectFallback = data.AllowDirectFallback;
                    Pool.Proxies = data.Proxies ?? new List<ProxyInfo>();
                }
            }
            OnProxyListChanged();
        }

        /// <summary>
        /// Asynchronously loads proxies from the active storage provider into this manager's pool.
        /// </summary>
        public async Task LoadFromStorageAsync()
        {
            var data = await ActiveStorage.LoadDataAsync().ConfigureAwait(false);
            lock (_lock)
            {
                if (data != null)
                {
                    IsEnabled = data.IsEnabled;
                    DefaultType = data.DefaultType;
                    Rotation = data.RotationMode;
                    Pool.RotateAfter = data.RotateAfter > 0 ? data.RotateAfter : 10;
                    Pool.AllowDirectFallback = data.AllowDirectFallback;
                    Pool.Proxies = data.Proxies ?? new List<ProxyInfo>();
                }
            }
            OnProxyListChanged();
        }

        /// <summary>
        /// Persists the current proxy pool to the active storage provider.
        /// </summary>
        public void SaveToStorage()
        {
            lock (_lock)
            {
                var data = new ProxyStorageData
                {
                    IsEnabled = IsEnabled,
                    DefaultType = DefaultType,
                    RotationMode = Rotation,
                    RotateAfter = Pool.RotateAfter,
                    AllowDirectFallback = AllowDirectFallback,
                    Proxies = Pool.Proxies.ToList()
                };
                ActiveStorage.SaveData(data);
            }
        }

        /// <summary>
        /// Asynchronously persists the current proxy pool to the active storage provider.
        /// </summary>
        public async Task SaveToStorageAsync()
        {
            ProxyStorageData data;
            lock (_lock)
            {
                data = new ProxyStorageData
                {
                    IsEnabled = IsEnabled,
                    DefaultType = DefaultType,
                    RotationMode = Rotation,
                    RotateAfter = Pool.RotateAfter,
                    AllowDirectFallback = AllowDirectFallback,
                    Proxies = Pool.Proxies.ToList()
                };
            }
            await ActiveStorage.SaveDataAsync(data).ConfigureAwait(false);
        }

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
            if (AutoSaveOnListChange) SaveToStorage();
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
            if (AutoSaveOnListChange) SaveToStorage();
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
            if (AutoSaveOnListChange) SaveToStorage();
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
            if (AutoSaveOnListChange) SaveToStorage();
            OnProxyListChanged();
        }

        /// <summary>
        /// Removes all proxies evaluated as dead (IsLive == false) from the manager.
        /// </summary>
        /// <returns>The number of removed dead proxies.</returns>
        public int RemoveDeadProxies()
        {
            int removed = Pool.RemoveDeadProxies();
            if (removed > 0)
            {
                if (AutoSaveOnListChange) SaveToStorage();
                OnProxyListChanged();
            }
            return removed;
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
        /// Starts a sticky session binding all HTTP requests in the current async execution context to a single proxy.
        /// Returns null if proxy routing is disabled or no proxy is available.
        /// </summary>
        public ProxySession? BeginSession(ProxyInfo? specificProxy = null)
        {
            if (!IsEnabled || Pool.Proxies.Count == 0) return null;
            var proxy = specificProxy ?? GetNext();
            if (proxy == null) return null;
            return new ProxySession(this, proxy);
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
        /// Creates an <see cref="IWebProxy"/> instance linked to this ProxyManager for dynamic request rotation.
        /// </summary>
        public IWebProxy CreateWebProxy()
        {
            return new DynamicWebProxy(this);
        }

        /// <summary>
        /// Creates an <see cref="HttpClientHandler"/> configured with dynamic proxy rotation.
        /// </summary>
        public HttpClientHandler CreateHandler(ProxyInfo? proxy = null, string? sessionKey = null)
        {
            if (!IsEnabled)
            {
                return new HttpClientHandler();
            }

            if (proxy != null)
            {
                return proxy.CreateHandler();
            }

            var handler = new HttpClientHandler();
            handler.Proxy = CreateWebProxy();
            return handler;
        }

        /// <summary>
        /// Saves proxy list to specific JSON file path.
        /// </summary>
        public void Save(string path)
        {
            var jsonStorage = new JsonFileStorage(path);
            lock (_lock)
            {
                jsonStorage.Save(Pool.Proxies);
            }
        }

        /// <summary>
        /// Loads proxy list from specific JSON file path.
        /// </summary>
        public void Load(string path)
        {
            var jsonStorage = new JsonFileStorage(path);
            var loaded = jsonStorage.Load();
            lock (_lock)
            {
                Pool.Proxies = loaded;
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

