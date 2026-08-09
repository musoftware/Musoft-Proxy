# Musoft.Proxy (ProxyForge) 🚀

[![NuGet Version](https://img.shields.io/nuget/v/Musoft.Proxy.svg?style=flat-square&color=blue)](https://www.nuget.org/packages/Musoft.Proxy)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)
[![.NET Supported](https://img.shields.io/badge/.NET-4.6.2%20%7C%20Standard%202.0%20%7C%206.0%20%7C%208.0-purple.svg?style=flat-square)](https://dotnet.microsoft.com/)

**Ultimate Proxy Manager - Rotation, Health Checker, Free Scraper, Ban Detector & WinForms Control for .NET.**  
Supports **HTTP**, **SOCKS5**, intelligent bulk import, parallel auto-checker, ban detection, free proxy scrapers, thread-safe rotation strategies (Round-Robin, Random, Least Latency, Sticky Session), Selenium & HttpClient integration, and a ready-to-use drop-in WinForms `UserControl` / `Form`.

---

## 📦 NuGet Packages

| Package | Description | NuGet |
| :--- | :--- | :--- |
| **`Musoft.Proxy`** | Core library: rotation strategies, health checks, ban detector, proxy parser, free scraper, judges | [![NuGet](https://img.shields.io/nuget/v/Musoft.Proxy.svg)](https://www.nuget.org/packages/Musoft.Proxy) |
| **`Musoft.Proxy.Extensions`** | Integration extensions for `HttpClient`, `SocketsHttpHandler`, and Selenium `ChromeOptions` | [![NuGet](https://img.shields.io/nuget/v/Musoft.Proxy.Extensions.svg)](https://www.nuget.org/packages/Musoft.Proxy.Extensions) |
| **`Musoft.Proxy.WinForms`** | WinForms `UserControl` & `Form` with grid UI, import/export, and health check dashboard | [![NuGet](https://img.shields.io/nuget/v/Musoft.Proxy.WinForms.svg)](https://www.nuget.org/packages/Musoft.Proxy.WinForms) |

---

## ⚡ Installation

Install via dotnet CLI:

```bash
# Core Proxy Management Library
dotnet add package Musoft.Proxy

# Extensions for Selenium & HttpClient
dotnet add package Musoft.Proxy.Extensions

# Complete WinForms UI Control
dotnet add package Musoft.Proxy.WinForms
```

Or via Package Manager Console in Visual Studio:

```powershell
Install-Package Musoft.Proxy
Install-Package Musoft.Proxy.Extensions
Install-Package Musoft.Proxy.WinForms
```

---

## 🔥 Key Features

- ⚡ **Zero-Config Core & Drop-in WinForms UI**: Add `Musoft.Proxy` to any .NET project or drop `ProxyManagerControl` directly into Visual Studio Toolbox.
- 🌐 **HTTP & SOCKS5 Support**: Seamless handling of HTTP, HTTPS, and SOCKS5 proxies (with authentication).
- 🧠 **Smart Proxy Parser**: Universal format recognition (`host:port`, `host:port:user:pass`, `user:pass@host:port`, `http://...`, `socks5://...`). Automatically strips inline/block comments (`#`, `//`) and whitespace.
- 🔄 **Advanced Rotation Strategies**: Round-Robin, Random, Least Latency, and Sticky Session strategies built for high-concurrency multi-threaded apps.
- 🚀 **Parallel Health Checker & Latency Test**: High-performance async non-blocking proxy validator with progress reporting.
- 🛡️ **Ban Detector & Proxy Judge**: Detect rate-limits, IP bans, HTTP status codes, block pages, and judge proxy header leaks.
- 🌐 **Free Proxy Scraper**: Built-in scraper for gathering public proxies automatically.
- 🔌 **HttpClient & Selenium Extensions**: Effortless integration with standard .NET `HttpClient` and Selenium WebDriver.
- 🧩 **Multi-Targeting**: `.NET Framework 4.6.2`, `.NET Standard 2.0`, `.NET 6.0`, `.NET 8.0` (and WinForms targets).

---

## 💻 Code Examples & Quick Start

### 1. Basic Proxy Rotation & Management (`Musoft.Proxy`)

```csharp
using ProxyForge.Core;

// Create pool and add proxies
var pool = new ProxyPool(RotationMode.RoundRobin);
pool.Add(new ProxyInfo("192.168.1.1", 8080, ProxyType.HTTP));
pool.Add(new ProxyInfo("192.168.1.2", 8080, ProxyType.SOCKS5, "user", "pass"));

// Retrieve next rotated proxy
ProxyInfo proxy = pool.GetNext();
Console.WriteLine($"Using proxy: {proxy.Host}:{proxy.Port}");
```

### 2. Intelligent Bulk Parsing

```csharp
using ProxyForge.Core;

string rawInput = @"
    # Bulk Proxies List
    192.168.1.1:8080
    192.168.1.2:8080:admin:secret123
    user:pass@192.168.1.3:8080
    socks5://10.0.0.1:1080
    socks5://admin:secret@10.0.0.2:1080 // inline comments supported
";

List<ProxyInfo> proxies = ProxyParser.Parse(rawInput, ProxyType.HTTP);
```

### 3. HttpClient & Rotated Request (`Musoft.Proxy.Extensions`)

```csharp
using System.Net.Http;
using ProxyForge.Core;
using ProxyForge.Extensions;

var manager = new ProxyManager();
manager.AddRange(proxies);

// Auto-create HttpClient with rotated proxy handler
using var client = manager.CreateHttpClient();
string ip = await client.GetStringAsync("https://api.ipify.org?format=json");
```

### 4. Selenium Integration (`Musoft.Proxy.Extensions`)

```csharp
using OpenQA.Selenium.Chrome;
using ProxyForge.Core;
using ProxyForge.Extensions;

var options = new ChromeOptions();
var proxy = new ProxyInfo("192.168.1.1", 8080, ProxyType.HTTP, "user", "pass");

options.ApplyProxy(proxy);
```

### 5. WinForms Drop-in Control (`Musoft.Proxy.WinForms`)

Drag `ProxyManagerControl` onto your Visual Studio Form, or use programmatically:

```csharp
using ProxyForge.WinForms;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        
        proxyControl.Manager.DefaultType = ProxyType.HTTP;
        proxyControl.Manager.Rotation = RotationMode.RoundRobin;
    }
}
```

---

## 🛠️ Solution Architecture

```
/src/ProxyForge.Core/       # Core Engine (Musoft.Proxy - Parsing, Rotation, Health, Scraper)
/src/ProxyForge.Extensions/ # Integration Extensions (Musoft.Proxy.Extensions - HttpClient & Selenium)
/src/ProxyForge.WinForms/   # WinForms UI Control (Musoft.Proxy.WinForms - UserControl & Form)
/demo/DemoApp/              # Executable WinForms Demo Application
```

---

## 📄 License

Distributed under the **MIT License**. See `LICENSE` for details.
