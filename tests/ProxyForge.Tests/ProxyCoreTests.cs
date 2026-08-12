using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ProxyForge.Core;
using Xunit;

namespace ProxyForge.Tests
{
    [Collection("StorageTests")]
    public class ProxyCoreTests
    {
        [Fact]
        public void ProxyParser_ParsesAllSupportedFormats()
        {
            string raw = @"
                # Standard format
                192.168.1.1:8080
                # Auth format host:port:user:pass
                10.0.0.1:1080:admin:secret123
                # Auth format user:pass:host:port
                user2:pass2:10.0.0.2:8080
                # Scheme format
                socks5://user3:pass3@10.0.0.3:1080 // inline comment
                http://10.0.0.4:8080 # another inline comment
            ";

            var list = ProxyParser.Parse(raw);

            Assert.Equal(5, list.Count);
            Assert.Equal("192.168.1.1", list[0].Host);
            Assert.Equal(8080, list[0].Port);

            Assert.Equal("10.0.0.1", list[1].Host);
            Assert.Equal(1080, list[1].Port);
            Assert.Equal("admin", list[1].Username);
            Assert.Equal("secret123", list[1].Password);

            Assert.Equal("10.0.0.2", list[2].Host);
            Assert.Equal(8080, list[2].Port);
            Assert.Equal("user2", list[2].Username);
            Assert.Equal("pass2", list[2].Password);

            Assert.Equal("10.0.0.3", list[3].Host);
            Assert.Equal(1080, list[3].Port);
            Assert.Equal(ProxyType.SOCKS5, list[3].Type);
            Assert.Equal("user3", list[3].Username);

            Assert.Equal("10.0.0.4", list[4].Host);
            Assert.Equal(8080, list[4].Port);
        }

        [Fact]
        public void ProxyPool_RotatesProxies_RoundRobin()
        {
            var pool = new ProxyPool
            {
                Mode = RotationMode.EveryRequest
            };

            var p1 = new ProxyInfo("1.1.1.1", 8080);
            var p2 = new ProxyInfo("2.2.2.2", 8080);
            pool.Proxies = new List<ProxyInfo> { p1, p2 };

            var first = pool.GetProxy();
            var second = pool.GetProxy();
            var third = pool.GetProxy();

            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.NotNull(third);

            Assert.NotEqual(first.Host, second.Host);
            Assert.Equal(first.Host, third.Host);
        }

        [Fact]
        public void ProxyPool_MarkFailed_PutsProxyInCooldown_OnFailure()
        {
            var pool = new ProxyPool
            {
                CooldownDuration = TimeSpan.FromMinutes(5)
            };

            var p1 = new ProxyInfo("1.1.1.1", 8080);
            pool.Proxies = new List<ProxyInfo> { p1 };

            pool.MarkFailed(p1);
            Assert.True(p1.IsInCooldown);
            Assert.Equal(1, p1.FailCount);
        }

        [Fact]
        public void ProxyPool_RotatesAfter_MaxSuccessfulRequests()
        {
            var pool = new ProxyPool
            {
                MaxSuccessfulRequestsPerProxy = 8
            };

            var p1 = new ProxyInfo("1.1.1.1", 8080);
            var p2 = new ProxyInfo("2.2.2.2", 8080);
            pool.Proxies = new List<ProxyInfo> { p1, p2 };

            var current = pool.GetProxy();
            Assert.NotNull(current);
            Assert.Equal("1.1.1.1", current.Host);

            // Simulate 7 successful requests
            for (int i = 0; i < 7; i++)
            {
                pool.MarkSuccess(p1);
            }
            Assert.Equal(7, p1.SuccessfulRequestsCount);
            Assert.Equal("1.1.1.1", pool.CurrentProxy?.Host);

            // 8th successful request triggers rotation!
            pool.MarkSuccess(p1);
            Assert.Equal(0, p1.SuccessfulRequestsCount);
            Assert.Equal("2.2.2.2", pool.CurrentProxy?.Host);
        }

        [Fact]
        public void BanDetector_DetectsBannedStatusCodesAndKeywords()
        {
            var detector = new BanDetector();

            using var respForbidden = new HttpResponseMessage(HttpStatusCode.Forbidden);
            Assert.True(detector.IsBanned(respForbidden));

            using var respOk = new HttpResponseMessage(HttpStatusCode.OK);
            Assert.False(detector.IsBanned(respOk));

            Assert.True(detector.IsBanned("Error 403: Cloudflare Access Denied"));
            Assert.True(detector.IsBanned("Please solve the captcha challenge below"));
            Assert.False(detector.IsBanned("Welcome to the website!"));
        }

        [Fact]
        public void ProxyFactory_CreateHandler_CreatesValidHandler()
        {
            var proxyHttp = new ProxyInfo("127.0.0.1", 8080, "user", "pass", ProxyType.HTTP);
            var handlerHttp = ProxyFactory.CreateHandler(proxyHttp);
            Assert.NotNull(handlerHttp);
            Assert.NotNull(handlerHttp.Proxy);

            var proxySocks = new ProxyInfo("127.0.0.1", 1080, ProxyType.SOCKS5);
            var handlerSocks = ProxyFactory.CreateHandler(proxySocks);
            Assert.NotNull(handlerSocks);
            Assert.NotNull(handlerSocks.Proxy);
        }

        [Fact]
        public void ProxyInfo_EqualsAndGetHashCode_WorkCorrectly()
        {
            var p1 = new ProxyInfo("192.168.1.1", 8080, "user", "pass", ProxyType.HTTP);
            var p2 = new ProxyInfo("192.168.1.1", 8080, "user", "pass", ProxyType.HTTP);
            var p3 = new ProxyInfo("192.168.1.2", 8080, "user", "pass", ProxyType.HTTP);

            Assert.Equal(p1, p2);
            Assert.Equal(p1.GetHashCode(), p2.GetHashCode());
            Assert.NotEqual(p1, p3);
        }

        [Fact]
        public void DynamicWebProxy_SynchronizesGetProxyAndCredentials()
        {
            var manager = new ProxyManager();
            manager.Clear();
            var proxy = new ProxyInfo("10.0.0.1", 8080, "myuser", "mypass");
            manager.Add(proxy);

            var webProxy = manager.CreateWebProxy();
            var targetUri = new Uri("http://example.com");

            var resolvedUri = webProxy.GetProxy(targetUri);
            Assert.NotNull(resolvedUri);
            Assert.Equal("http://10.0.0.1:8080/", resolvedUri.ToString());

            var creds = webProxy.Credentials;
            Assert.NotNull(creds);
            var networkCred = creds.GetCredential(resolvedUri, "Basic");
            Assert.NotNull(networkCred);
            Assert.Equal("myuser", networkCred.UserName);
            Assert.Equal("mypass", networkCred.Password);
        }

        [Fact]
        public void ProxyPool_GetProxy_PrioritizesWorkingOverUntested_AndExcludesDead()
        {
            var pool = new ProxyPool
            {
                Mode = RotationMode.EveryRequest
            };

            var deadProxy = new ProxyInfo("1.1.1.1", 8080) { IsLive = false };
            var untestedProxy = new ProxyInfo("2.2.2.2", 8080) { IsLive = null };
            var workingProxy = new ProxyInfo("3.3.3.3", 8080) { IsLive = true };

            pool.Proxies = new List<ProxyInfo> { deadProxy, untestedProxy, workingProxy };

            // 1. Should pick workingProxy (3.3.3.3) over untested and dead proxies
            var selected = pool.GetProxy();
            Assert.NotNull(selected);
            Assert.Equal("3.3.3.3", selected.Host);

            // 2. If workingProxy is removed/dead, should fall back to untestedProxy (2.2.2.2)
            pool.Proxies.Remove(workingProxy);
            selected = pool.GetProxy();
            Assert.NotNull(selected);
            Assert.Equal("2.2.2.2", selected.Host);
        }

        [Fact]
        public void ProxyManager_RemoveDeadProxies_RemovesOnlyDeadProxies()
        {
            var manager = new ProxyManager();
            manager.Clear();

            var liveProxy = new ProxyInfo("1.1.1.1", 8080) { IsLive = true };
            var deadProxy = new ProxyInfo("2.2.2.2", 8080) { IsLive = false };
            var untestedProxy = new ProxyInfo("3.3.3.3", 8080) { IsLive = null };

            manager.AddRange(new[] { liveProxy, deadProxy, untestedProxy });
            Assert.Equal(3, manager.Proxies.Count);

            int removed = manager.RemoveDeadProxies();
            Assert.Equal(1, removed);
            Assert.Equal(2, manager.Proxies.Count);
            Assert.DoesNotContain(deadProxy, manager.Proxies);
            Assert.Contains(liveProxy, manager.Proxies);
            Assert.Contains(untestedProxy, manager.Proxies);
        }
    }
}
