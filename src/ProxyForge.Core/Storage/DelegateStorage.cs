using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProxyForge.Core
{
    /// <summary>
    /// Custom delegate storage provider implementation of <see cref="IProxyStorage"/> for custom API or DB integration.
    /// </summary>
    public class DelegateStorage : IProxyStorage
    {
        private readonly Func<List<ProxyInfo>>? _loadFunc;
        private readonly Action<IEnumerable<ProxyInfo>>? _saveAction;
        private readonly Func<Task<List<ProxyInfo>>>? _loadAsyncFunc;
        private readonly Func<IEnumerable<ProxyInfo>, Task>? _saveAsyncFunc;

        public StorageMethod Method => StorageMethod.CustomApi;

        /// <summary>
        /// Initializes custom delegate storage using synchronous load and save actions.
        /// </summary>
        public DelegateStorage(Func<List<ProxyInfo>> loadFunc, Action<IEnumerable<ProxyInfo>> saveAction)
        {
            _loadFunc = loadFunc ?? throw new ArgumentNullException(nameof(loadFunc));
            _saveAction = saveAction ?? throw new ArgumentNullException(nameof(saveAction));
        }

        /// <summary>
        /// Initializes custom delegate storage using asynchronous load and save functions.
        /// </summary>
        public DelegateStorage(Func<Task<List<ProxyInfo>>> loadAsyncFunc, Func<IEnumerable<ProxyInfo>, Task> saveAsyncFunc)
        {
            _loadAsyncFunc = loadAsyncFunc ?? throw new ArgumentNullException(nameof(loadAsyncFunc));
            _saveAsyncFunc = saveAsyncFunc ?? throw new ArgumentNullException(nameof(saveAsyncFunc));
        }

        public List<ProxyInfo> Load()
        {
            if (_loadFunc != null)
            {
                return _loadFunc() ?? new List<ProxyInfo>();
            }
            if (_loadAsyncFunc != null)
            {
                return Task.Run(async () => await _loadAsyncFunc().ConfigureAwait(false)).GetAwaiter().GetResult() ?? new List<ProxyInfo>();
            }
            return new List<ProxyInfo>();
        }

        public async Task<List<ProxyInfo>> LoadAsync()
        {
            if (_loadAsyncFunc != null)
            {
                return await _loadAsyncFunc().ConfigureAwait(false) ?? new List<ProxyInfo>();
            }
            if (_loadFunc != null)
            {
                return _loadFunc() ?? new List<ProxyInfo>();
            }
            return new List<ProxyInfo>();
        }

        public void Save(IEnumerable<ProxyInfo> proxies)
        {
            if (_saveAction != null)
            {
                _saveAction(proxies);
            }
            else if (_saveAsyncFunc != null)
            {
                Task.Run(async () => await _saveAsyncFunc(proxies).ConfigureAwait(false)).GetAwaiter().GetResult();
            }
        }

        public async Task SaveAsync(IEnumerable<ProxyInfo> proxies)
        {
            if (_saveAsyncFunc != null)
            {
                await _saveAsyncFunc(proxies).ConfigureAwait(false);
            }
            else if (_saveAction != null)
            {
                _saveAction(proxies);
            }
        }

        public ProxyStorageData LoadData()
        {
            return new ProxyStorageData { Proxies = Load() };
        }

        public async Task<ProxyStorageData> LoadDataAsync()
        {
            var proxies = await LoadAsync().ConfigureAwait(false);
            return new ProxyStorageData { Proxies = proxies };
        }

        public void SaveData(ProxyStorageData data)
        {
            if (data?.Proxies != null)
            {
                Save(data.Proxies);
            }
        }

        public async Task SaveDataAsync(ProxyStorageData data)
        {
            if (data?.Proxies != null)
            {
                await SaveAsync(data.Proxies).ConfigureAwait(false);
            }
        }
    }
}
