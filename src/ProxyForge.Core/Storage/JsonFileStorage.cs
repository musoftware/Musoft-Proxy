using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProxyForge.Core
{
    /// <summary>
    /// Local JSON file storage implementation of <see cref="IProxyStorage"/>.
    /// </summary>
    public class JsonFileStorage : IProxyStorage
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private static readonly ConcurrentDictionary<string, object> FileLocks = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the target JSON file path.
        /// </summary>
        public string FilePath { get; }

        public StorageMethod Method => StorageMethod.JsonFile;

        private object GetFileLock()
        {
            string key = Path.GetFullPath(FilePath);
            return FileLocks.GetOrAdd(key, _ => new object());
        }

        /// <summary>
        /// Initializes a new instance of <see cref="JsonFileStorage"/>.
        /// </summary>
        /// <param name="filePath">Path to JSON persistence file. Defaults to "proxies.json".</param>
        public JsonFileStorage(string filePath = "proxies.json")
        {
            string targetPath = string.IsNullOrWhiteSpace(filePath) ? "proxies.json" : filePath;
            if (!Path.IsPathRooted(targetPath))
            {
                targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, targetPath);
            }
            FilePath = targetPath;
        }

        public ProxyStorageData LoadData()
        {
            lock (GetFileLock())
            {
                if (!File.Exists(FilePath))
                {
                    return new ProxyStorageData();
                }

                try
                {
                    using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream);
                    string json = reader.ReadToEnd();
                    string trimmed = json.TrimStart();
                    if (trimmed.StartsWith("["))
                    {
                        var list = JsonSerializer.Deserialize<List<ProxyInfo>>(json, JsonOptions);
                        return new ProxyStorageData { Proxies = list ?? new List<ProxyInfo>() };
                    }
                    else
                    {
                        var data = JsonSerializer.Deserialize<ProxyStorageData>(json, JsonOptions);
                        return data ?? new ProxyStorageData();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"JsonFileStorage LoadData error: {ex.Message}");
                    return new ProxyStorageData();
                }
            }
        }

        public async Task<ProxyStorageData> LoadDataAsync()
        {
            if (!File.Exists(FilePath))
            {
                return new ProxyStorageData();
            }

            try
            {
                using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, useAsync: true);
                using var reader = new StreamReader(stream);
                string json = await reader.ReadToEndAsync().ConfigureAwait(false);
                string trimmed = json.TrimStart();
                if (trimmed.StartsWith("["))
                {
                    var list = JsonSerializer.Deserialize<List<ProxyInfo>>(json, JsonOptions);
                    return new ProxyStorageData { Proxies = list ?? new List<ProxyInfo>() };
                }
                else
                {
                    var data = JsonSerializer.Deserialize<ProxyStorageData>(json, JsonOptions);
                    return data ?? new ProxyStorageData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JsonFileStorage LoadDataAsync error: {ex.Message}");
                return new ProxyStorageData();
            }
        }

        public void SaveData(ProxyStorageData data)
        {
            lock (GetFileLock())
            {
                try
                {
                    string json = JsonSerializer.Serialize(data, JsonOptions);
                    string? dir = Path.GetDirectoryName(FilePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    using var stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    using var writer = new StreamWriter(stream);
                    writer.Write(json);
                    writer.Flush();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"JsonFileStorage SaveData error: {ex.Message}");
                }
            }
        }

        public async Task SaveDataAsync(ProxyStorageData data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data, JsonOptions);
                string? dir = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using var stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
                using var writer = new StreamWriter(stream);
                await writer.WriteAsync(json).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JsonFileStorage SaveDataAsync error: {ex.Message}");
            }
        }

        public List<ProxyInfo> Load()
        {
            return LoadData().Proxies;
        }

        public async Task<List<ProxyInfo>> LoadAsync()
        {
            var data = await LoadDataAsync().ConfigureAwait(false);
            return data.Proxies;
        }

        public void Save(IEnumerable<ProxyInfo> proxies)
        {
            var data = LoadData();
            data.Proxies = proxies.ToList();
            SaveData(data);
        }

        public async Task SaveAsync(IEnumerable<ProxyInfo> proxies)
        {
            var data = await LoadDataAsync().ConfigureAwait(false);
            data.Proxies = proxies.ToList();
            await SaveDataAsync(data).ConfigureAwait(false);
        }
    }
}
