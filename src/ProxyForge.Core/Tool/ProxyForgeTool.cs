using System;
using System.IO;
using System.Threading.Tasks;

namespace ProxyForge.Core
{
    /// <summary>
    /// Simple command-line tool utility for checking proxies from a file.
    /// </summary>
    public static class ProxyForgeTool
    {
        /// <summary>
        /// Console entry point supporting: check &lt;filePath&gt;
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static async Task Main(string[] args)
        {
            if (args == null || args.Length < 2 || !string.Equals(args[0], "check", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Usage: ProxyForgeTool check <filePath>");
                return;
            }

            string filePath = args[1];
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found: '{filePath}'");
                return;
            }

            Console.WriteLine($"Loading proxies from '{filePath}'...");
            var provider = new FileProxyProvider(filePath);
            var proxies = await provider.FetchAsync().ConfigureAwait(false);

            if (proxies.Count == 0)
            {
                Console.WriteLine("No valid proxies found in file.");
                return;
            }

            Console.WriteLine($"Found {proxies.Count} proxies. Starting health check...");

            var checker = new ProxyHealthChecker();
            checker.OnProxyAlive += (s, e) =>
            {
                Console.WriteLine($"[LIVE] {e.Proxy.Host}:{e.Proxy.Port} | Latency: {e.Result.LatencyMs}ms | Type: {e.Proxy.Type}");
            };
            checker.OnProxyDead += (s, e) =>
            {
                Console.WriteLine($"[DEAD] {e.Proxy.Host}:{e.Proxy.Port} | Error: {e.Result.ErrorMessage}");
            };

            await checker.CheckAllAsync(proxies, maxParallel: 20).ConfigureAwait(false);

            Console.WriteLine("Health check completed.");
        }
    }
}
