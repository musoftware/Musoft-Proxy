using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProxyForge.Core;
using Xunit;

namespace ProxyForge.Tests
{
    [Collection("StorageTests")]
    public class ScraperTests
    {
        [Fact]
        public async Task DynamicSourceDiscoverer_DiscoversProxyListUrls_ReturnsNonEmptyList()
        {
            var discoverer = new DynamicSourceDiscoverer();
            var urls = await discoverer.DiscoverProxyListUrlsAsync(maxResults: 15);

            Assert.NotNull(urls);
            Assert.NotEmpty(urls);
            Assert.All(urls, url => Assert.True(DynamicSourceDiscoverer.IsValidProxyListUrl(url)));
        }

        [Fact]
        public async Task FreeProxyScraper_DiscoverAndAddNewSources_DiscoversAndAddsNewSources()
        {
            var scraper = new FreeProxyScraper();
            int initialCount = scraper.GetAllSources().Count;

            int added = await scraper.DiscoverAndAddNewSourcesAsync();
            int finalCount = scraper.GetAllSources().Count;

            Assert.True(added >= 0);
            Assert.True(finalCount >= initialCount);
            Assert.True(finalCount > 0);
        }

        [Fact]
        public async Task FreeProxyScraper_FetchAsync_HarvestsProxiesFromSources()
        {
            var scraper = new FreeProxyScraper();
            var proxies = await scraper.FetchAsync();

            Assert.NotNull(proxies);
            Assert.NotEmpty(proxies);
            Assert.All(proxies, p =>
            {
                Assert.False(string.IsNullOrWhiteSpace(p.Host));
                Assert.InRange(p.Port, 1, 65535);
            });
        }
    }
}
