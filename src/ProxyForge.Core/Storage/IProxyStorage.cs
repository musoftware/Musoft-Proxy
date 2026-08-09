using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProxyForge.Core
{
    /// <summary>
    /// Contract for proxy storage mechanisms (In-Memory, JSON File, Custom API, etc.).
    /// </summary>
    public interface IProxyStorage
    {
        /// <summary>
        /// Gets the storage method type associated with this provider.
        /// </summary>
        StorageMethod Method { get; }

        /// <summary>
        /// Synchronously loads the list of proxies from storage.
        /// </summary>
        List<ProxyInfo> Load();

        /// <summary>
        /// Asynchronously loads the list of proxies from storage.
        /// </summary>
        Task<List<ProxyInfo>> LoadAsync();

        /// <summary>
        /// Synchronously persists the list of proxies to storage.
        /// </summary>
        void Save(IEnumerable<ProxyInfo> proxies);

        /// <summary>
        /// Synchronously loads the complete storage payload data including configuration.
        /// </summary>
        ProxyStorageData LoadData();

        /// <summary>
        /// Asynchronously loads the complete storage payload data including configuration.
        /// </summary>
        Task<ProxyStorageData> LoadDataAsync();

        /// <summary>
        /// Synchronously persists the complete storage payload data including configuration.
        /// </summary>
        void SaveData(ProxyStorageData data);

        /// <summary>
        /// Asynchronously persists the complete storage payload data including configuration.
        /// </summary>
        Task SaveDataAsync(ProxyStorageData data);
    }
}
