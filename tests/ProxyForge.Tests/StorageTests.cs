using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ProxyForge.Core;
using Xunit;

namespace ProxyForge.Tests
{
    [Collection("StorageTests")]
    public class StorageTests : IDisposable
    {
        private readonly string _testJsonFile = Path.Combine(Path.GetTempPath(), $"proxies_test_{Guid.NewGuid():N}.json");

        public StorageTests()
        {
            // Reset to InMemory storage before each test
            ProxyManager.SetStorageMethod(StorageMethod.InMemory);
            ProxyManager.Default.Clear();
        }

        public void Dispose()
        {
            if (File.Exists(_testJsonFile))
            {
                try { File.Delete(_testJsonFile); } catch { }
            }
        }

        [Fact]
        public void SetStorageMethod_InMemory_WorksCorrectly()
        {
            ProxyManager.SetStorageMethod(StorageMethod.InMemory);
            Assert.Equal(StorageMethod.InMemory, ProxyManager.StorageProvider.Method);

            var manager = new ProxyManager();
            manager.Clear();

            var proxy = new ProxyInfo("127.0.0.1", 8080);
            manager.Add(proxy);

            Assert.Single(manager.Proxies);
            Assert.Equal("127.0.0.1", manager.Proxies[0].Host);
        }

        [Fact]
        public void SetStorageMethod_JsonFile_AutoSavesAndLoads()
        {
            ProxyManager.SetStorageMethod(StorageMethod.JsonFile, _testJsonFile);
            Assert.Equal(StorageMethod.JsonFile, ProxyManager.StorageProvider.Method);

            var manager1 = new ProxyManager();
            manager1.Clear();
            manager1.Add(new ProxyInfo("192.168.1.100", 8080));
            manager1.Add(new ProxyInfo("10.0.0.1", 1080, ProxyType.SOCKS5));

            Assert.True(File.Exists(_testJsonFile));

            // Create new instance which should auto-load from json storage
            var manager2 = new ProxyManager();
            Assert.Equal(2, manager2.Proxies.Count);
            Assert.Equal("192.168.1.100", manager2.Proxies[0].Host);
            Assert.Equal("10.0.0.1", manager2.Proxies[1].Host);
            Assert.Equal(ProxyType.SOCKS5, manager2.Proxies[1].Type);
        }

        [Fact]
        public void SetStorageMethod_CustomApi_Delegates_WorkCorrectly()
        {
            var fakeDatabase = new List<ProxyInfo>();
            fakeDatabase.Add(new ProxyInfo("1.1.1.1", 80));

            ProxyManager.SetStorageMethod(
                loadFunc: () => fakeDatabase,
                saveAction: (proxies) =>
                {
                    fakeDatabase.Clear();
                    fakeDatabase.AddRange(proxies);
                }
            );

            Assert.Equal(StorageMethod.CustomApi, ProxyManager.StorageProvider.Method);

            var manager = new ProxyManager();
            Assert.Single(manager.Proxies);
            Assert.Equal("1.1.1.1", manager.Proxies[0].Host);

            manager.Add(new ProxyInfo("2.2.2.2", 8080));
            Assert.Equal(2, fakeDatabase.Count);
            Assert.Equal("2.2.2.2", fakeDatabase[1].Host);
        }

        [Fact]
        public async Task SetStorageMethod_CustomApi_AsyncDelegates_WorkCorrectly()
        {
            var fakeDatabase = new List<ProxyInfo> { new ProxyInfo("8.8.8.8", 53) };

            ProxyManager.SetStorageMethod(
                loadAsyncFunc: () => Task.FromResult(fakeDatabase),
                saveAsyncFunc: (proxies) =>
                {
                    fakeDatabase.Clear();
                    fakeDatabase.AddRange(proxies);
                    return Task.CompletedTask;
                }
            );

            var manager = new ProxyManager();
            Assert.Single(manager.Proxies);
            Assert.Equal("8.8.8.8", manager.Proxies[0].Host);

            manager.Add(new ProxyInfo("9.9.9.9", 8080));
            await manager.SaveToStorageAsync();

            Assert.Equal(2, fakeDatabase.Count);
            Assert.Equal("9.9.9.9", fakeDatabase[1].Host);
        }
    }
}
