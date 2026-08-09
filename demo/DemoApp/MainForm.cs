using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartProxyManager.Core;

namespace DemoApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            // Example: Pre-seed sample proxies for demo purposes
            var sampleProxies = ProxyParser.Parse(@"
                # Sample proxies format demonstration
                127.0.0.1:8080
                http://user:secret123@192.168.1.50:8080
                socks5://10.0.0.5:1080
            ", ProxyType.HTTP);

            proxyControl.Manager.AddRange(sampleProxies);
        }

        private async void btnTestHttpClient_Click(object sender, EventArgs e)
        {
            btnTestHttpClient.Enabled = false;
            txtOutput.Text = "Sending HTTP request via ProxyManager.CreateHandler()...";

            try
            {
                // Obtain a HttpClientHandler created from proxyControl.Manager
                using var handler = proxyControl.Manager.CreateHandler();
                using var client = new HttpClient(handler, disposeHandler: true);
                client.Timeout = TimeSpan.FromSeconds(8);

                var response = await client.GetAsync("https://api.ipify.org?format=json");
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    txtOutput.Text = $"Success! IP Response: {content.Trim()}";
                }
                else
                {
                    txtOutput.Text = $"Request failed: HTTP {(int)response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                txtOutput.Text = $"HttpClient Error: {ex.Message}";
            }
            finally
            {
                btnTestHttpClient.Enabled = true;
            }
        }
    }
}
