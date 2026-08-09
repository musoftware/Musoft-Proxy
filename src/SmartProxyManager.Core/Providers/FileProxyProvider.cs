using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SmartProxyManager.Core
{
    /// <summary>
    /// Proxy provider that loads proxies from a local text or log file.
    /// </summary>
    public class FileProxyProvider : IProxyProvider
    {
        /// <summary>
        /// Gets or sets the file path to read proxies from.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets default proxy type.
        /// </summary>
        public ProxyType DefaultType { get; set; } = ProxyType.HTTP;

        /// <summary>
        /// Initializes a new instance of <see cref="FileProxyProvider"/>.
        /// </summary>
        /// <param name="filePath">Target file path.</param>
        /// <param name="defaultType">Default proxy type.</param>
        public FileProxyProvider(string filePath, ProxyType defaultType = ProxyType.HTTP)
        {
            FilePath = filePath;
            DefaultType = defaultType;
        }

        /// <inheritdoc />
        public async Task<List<ProxyInfo>> FetchAsync()
        {
            if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath))
            {
                return new List<ProxyInfo>();
            }

            string content = await Task.Run(() => File.ReadAllText(FilePath)).ConfigureAwait(false);
            return ProxyParser.Parse(content, DefaultType);
        }
    }
}
