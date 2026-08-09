# ProxyForge.WinForms 🚀

[![NuGet Version](https://img.shields.io/nuget/v/ProxyForge.WinForms.svg?style=flat-square&color=blue)](https://www.nuget.org/packages/ProxyForge.WinForms)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)
[![.NET Supported](https://img.shields.io/badge/.NET-4.6.2%20%7C%206.0--windows%20%7C%208.0--windows-purple.svg?style=flat-square)](https://dotnet.microsoft.com/)

**The ultimate, zero-config WinForms proxy management control.**  
Supports **HTTP**, **SOCKS5**, intelligent bulk import, parallel auto-checker, thread-safe rotation, and latency testing out of the box. Drop-in standard `UserControl` or ready-to-use `Form`.

---

## 🌟 Demo Preview

![ProxyForge Control Preview](https://raw.githubusercontent.com/Musoftware/ProxyForge/main/assets/demo_preview.gif)

*(Placeholder: Drop the `ProxyManagerControl` into your Visual Studio toolbox or form layout)*

---

## 🔥 Features

- ⚡ **Zero-Config Drop-in Control**: Visual Studio Toolbox ready. Add `ProxyManagerControl` directly to any WinForms layout.
- 🌐 **HTTP & SOCKS5 Support**: Out-of-the-box support for HTTP, HTTPS, and SOCKS5 proxies (with authentication).
- 🧠 **Smart Proxy Parser**: Universal format recognition (`host:port`, `host:port:user:pass`, `user:pass@host:port`, `http://...`, `socks5://...`). Handles comments `#` or `//`, multi-line inputs, and spaces seamlessly.
- 🔄 **Thread-Safe Rotation**: Round-Robin and Random proxy rotation algorithms built for multi-threaded applications.
- 🚀 **Parallel Latency Tester**: Async non-blocking checker with configurable concurrency limits and color-coded UI indicators.
- 💾 **JSON Persistence**: Native `Save` / `Load` methods for proxy list state.
- 🧩 **Multi-Targeting**: `.NET Framework 4.6.2`, `.NET 6.0-windows`, and `.NET 8.0-windows`.

---

## 📦 Installation

Install via NuGet Package Manager Console or dotnet CLI:

```bash
dotnet add package ProxyForge.WinForms
```

Or via Package Manager Console in Visual Studio:

```powershell
Install-Package ProxyForge.WinForms
```

---

## 💻 Quick Start & Usage

### 1. Simple Drop-in Control (Form Integration)

Drag `ProxyManagerControl` onto your Form in Visual Studio or initialize programmatically:

```csharp
using ProxyForge.WinForms;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        
        // Access manager properties directly
        proxyControl.Manager.DefaultType = ProxyType.HTTP;
        proxyControl.Manager.Rotation = RotationMode.RoundRobin;
    }
}
```

### 2. Using ProxyManager with HttpClient

Generate a pre-configured `HttpClientHandler` instantly from your `ProxyManager`:

```csharp
using System.Net.Http;
using ProxyForge.Core;

// Retrieve the next rotated proxy automatically
var handler = proxyManagerControl.Manager.CreateHandler();
using var client = new HttpClient(handler, disposeHandler: true);

// Execute request using rotated proxy
string result = await client.GetStringAsync("https://api.ipify.org?format=json");
Console.WriteLine($"Outbound IP: {result}");
```

### 3. Intelligent Bulk Parsing (The Killer Feature)

Parse any input string format without worrying about formatting rules:

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
proxyControl.Manager.AddRange(proxies);
```

### 4. Asynchronous Testing & Latency Measurement

```csharp
using ProxyForge.Core;

// Test all proxies in parallel with progress callback
var progress = new Progress<ProxyInfo>(p => 
{
    Console.WriteLine($"{p.Host}:{p.Port} -> Live: {p.IsLive}, Latency: {p.LatencyMs}ms");
});

await ProxyTester.TestAllAsync(proxyControl.Manager.Proxies, maxParallel: 20, progress: progress);
```

### 5. Display Modal Proxy Manager Form

Need a quick modal window to manage proxies? Use `ProxyManagerForm`:

```csharp
using (var form = new ProxyManagerForm())
{
    if (form.ShowDialog() == DialogResult.OK)
    {
        var activeManager = form.Manager;
        // Use configured proxies
    }
}
```

---

## 🛠️ Solution Architecture

```
/src/ProxyForge.Core/       # Core Engine (Parsing, Rotation, Testing, Handlers)
/src/ProxyForge.WinForms/   # WinForms Control Library (UserControl & Form UI)
/demo/DemoApp/                      # Executable WinForms Demo Application
```

---

## 📄 License

Distributed under the **MIT License**. See `LICENSE` for more information.

---

⭐ **If you find ProxyForge useful, please consider giving this repository a star on GitHub!**
