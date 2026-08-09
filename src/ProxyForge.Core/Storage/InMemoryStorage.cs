using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyForge.Core
{
    /// <summary>
    /// In-memory storage implementation of <see cref="IProxyStorage"/>. Does not persist data outside memory.
    /// </summary>
    public class InMemoryStorage : IProxyStorage
    {
        private readonly List<ProxyInfo> _inMemoryList = new List<ProxyInfo>();
        private ProxyStorageData _storageData = new ProxyStorageData();
        private readonly object _lock = new object();

        public StorageMethod Method => StorageMethod.InMemory;

        public List<ProxyInfo> Load()
        {
            lock (_lock)
            {
                return _inMemoryList.ToList();
            }
        }

        public Task<List<ProxyInfo>> LoadAsync()
        {
            return Task.FromResult(Load());
        }

        public void Save(IEnumerable<ProxyInfo> proxies)
        {
            if (proxies == null) return;
            lock (_lock)
            {
                _inMemoryList.Clear();
                _inMemoryList.AddRange(proxies);
                _storageData.Proxies = _inMemoryList.ToList();
            }
        }

        public Task SaveAsync(IEnumerable<ProxyInfo> proxies)
        {
            Save(proxies);
            return Task.CompletedTask;
        }

        public ProxyStorageData LoadData()
        {
            lock (_lock)
            {
                _storageData.Proxies = _inMemoryList.ToList();
                return _storageData;
            }
        }

        public Task<ProxyStorageData> LoadDataAsync()
        {
            return Task.FromResult(LoadData());
        }

        public void SaveData(ProxyStorageData data)
        {
            if (data == null) return;
            lock (_lock)
            {
                _storageData = data;
                _inMemoryList.Clear();
                if (data.Proxies != null)
                {
                    _inMemoryList.AddRange(data.Proxies);
                }
            }
        }

        public Task SaveDataAsync(ProxyStorageData data)
        {
            SaveData(data);
            return Task.CompletedTask;
        }
    }
}
