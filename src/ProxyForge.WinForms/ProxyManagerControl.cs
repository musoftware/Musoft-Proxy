using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ProxyForge.Core;
using ProxyForge.WinForms.Resources;

namespace ProxyForge.WinForms
{
    /// <summary>
    /// Drop-in WinForms UserControl for comprehensive proxy management, bulk import, auto-checker, and rotation.
    /// </summary>
    public partial class ProxyManagerControl : UserControl
    {
        private bool _isTesting = false;

        /// <summary>
        /// Gets the underlying <see cref="ProxyManager"/> instance.
        /// </summary>
        public ProxyManager Manager { get; } = new ProxyManager();

        /// <summary>
        /// Occurs when the Save button is clicked.
        /// </summary>
        public event EventHandler? SaveClicked;

        /// <summary>
        /// Occurs when the Cancel button is clicked.
        /// </summary>
        public event EventHandler? CancelClicked;

        /// <summary>
        /// Occurs when a single proxy test finishes.
        /// </summary>
        public event EventHandler<ProxyInfo>? ProxyTested;

        /// <summary>
        /// Occurs when a user validation error occurs.
        /// </summary>
        public event EventHandler<string>? ValidationError;

        /// <summary>
        /// Initializes a new instance of <see cref="ProxyManagerControl"/>.
        /// </summary>
        public ProxyManagerControl()
        {
            InitializeComponent();

            InitializeDropdowns();
            Manager.ProxyListChanged += Manager_ProxyListChanged;
            Manager.Pool.OnProxyRotated += Pool_OnProxyRotated;

            RefreshListView();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RefreshListView();
        }

        /// <summary>
        /// Gets the rotation mode ComboBox control.
        /// </summary>
        public ComboBox cmbRotationMode => cmbRotation;

        private void InitializeDropdowns()
        {
            cmbProxyType.Items.Clear();
            cmbProxyType.Items.Add(ProxyType.HTTP);
            cmbProxyType.Items.Add(ProxyType.SOCKS5);
            cmbProxyType.SelectedItem = Manager.DefaultType;

            cmbRotation.Items.Clear();
            foreach (RotationMode mode in Enum.GetValues(typeof(RotationMode)))
            {
                cmbRotation.Items.Add(mode);
            }
            cmbRotation.SelectedItem = Manager.Rotation;

            numRotateAfter.Value = Manager.Pool.RotateAfter;
            chkEnableProxy.Checked = Manager.IsEnabled;
            UpdateStrategyLabel();
            UpdateCurrentProxyLabel();
        }

        private void Pool_OnProxyRotated(object? sender, ProxyRotatedEventArgs e)
        {
            SafeInvoke(() =>
            {
                UpdateStatusLabel();
                UpdateCurrentProxyLabel();
            });
        }

        private void UpdateCurrentProxyLabel()
        {
            var proxy = Manager.Pool.GetProxy();
            lblCurrentProxy.Text = $"Current: {(proxy != null ? proxy.ToString() : "None")}";
        }

        private void Manager_ProxyListChanged(object? sender, EventArgs e)
        {
            SafeInvoke(RefreshListView);
        }

        /// <summary>
        /// Rebuilds the ListView items based on <see cref="ProxyManager.Proxies"/>.
        /// </summary>
        public void RefreshListView()
        {
            lstProxies.BeginUpdate();
            lstProxies.Items.Clear();

            foreach (var p in Manager.Proxies.ToList())
            {
                var item = CreateListViewItem(p);
                lstProxies.Items.Add(item);
            }

            lstProxies.EndUpdate();
            UpdateStatusLabel();
        }

        private static void GetStatusInfo(ProxyInfo p, out string statusText, out Color statusColor)
        {
            statusText = Strings.StatusUnchecked;
            statusColor = Color.Gray;

            if (p.IsBanned)
            {
                statusText = "Banned 🚫";
                statusColor = Color.DarkRed;
            }
            else if (p.IsInCooldown)
            {
                statusText = "Cooldown ⏳";
                statusColor = Color.OrangeRed;
            }
            else if (p.IsLive.HasValue)
            {
                if (p.IsLive.Value)
                {
                    statusText = Strings.StatusLive + " 🟢";
                    statusColor = Color.DarkGreen;
                }
                else
                {
                    statusText = Strings.StatusDead + " 🔴";
                    statusColor = Color.Red;
                }
            }
        }

        private ListViewItem CreateListViewItem(ProxyInfo p)
        {
            GetStatusInfo(p, out string statusText, out Color statusColor);

            string latencyText = p.LatencyMs >= 0 ? $"{p.LatencyMs} ms" : "-";
            string countryText = string.IsNullOrEmpty(p.CountryCode) ? "-" : p.CountryCode;
            string anonymityText = string.IsNullOrEmpty(p.AnonymityLevel) ? "-" : p.AnonymityLevel;

            var item = new ListViewItem(p.Host)
            {
                Tag = p,
                UseItemStyleForSubItems = false
            };

            item.SubItems.Add(p.Port.ToString());
            item.SubItems.Add(string.IsNullOrEmpty(p.Username) ? "-" : p.Username);
            item.SubItems.Add(countryText);

            var anonSub = item.SubItems.Add(anonymityText);
            if (string.Equals(p.AnonymityLevel, ProxyConstants.Anonymity.Elite, StringComparison.OrdinalIgnoreCase))
            {
                anonSub.ForeColor = Color.DarkGreen;
                anonSub.Font = new Font(lstProxies.Font, FontStyle.Bold);
            }

            var statusSub = item.SubItems.Add(statusText);
            statusSub.ForeColor = statusColor;

            item.SubItems.Add(latencyText);
            item.SubItems.Add(p.Type.ToString());

            return item;
        }

        private void UpdateListViewItem(ProxyInfo proxy)
        {
            foreach (ListViewItem item in lstProxies.Items)
            {
                if (item.Tag is ProxyInfo p && ReferenceEquals(p, proxy))
                {
                    GetStatusInfo(proxy, out string statusText, out Color statusColor);

                    item.SubItems[3].Text = string.IsNullOrEmpty(proxy.CountryCode) ? "-" : proxy.CountryCode;
                    item.SubItems[4].Text = string.IsNullOrEmpty(proxy.AnonymityLevel) ? "-" : proxy.AnonymityLevel;
                    if (string.Equals(proxy.AnonymityLevel, ProxyConstants.Anonymity.Elite, StringComparison.OrdinalIgnoreCase))
                    {
                        item.SubItems[4].ForeColor = Color.DarkGreen;
                        item.SubItems[4].Font = new Font(lstProxies.Font, FontStyle.Bold);
                    }

                    item.SubItems[5].Text = statusText;
                    item.SubItems[5].ForeColor = statusColor;
                    item.SubItems[6].Text = proxy.LatencyMs >= 0 ? $"{proxy.LatencyMs} ms" : "-";
                    break;
                }
            }
        }

        private void UpdateStatusLabel()
        {
            int total = Manager.Proxies.Count;
            int live = Manager.Proxies.Count(p => p.IsLive == true);
            int dead = Manager.Proxies.Count(p => p.IsLive == false);

            if (_isTesting) return;

            if (total == 0)
            {
                lblTestStatus.Text = Strings.StatusReady;
                lblTestStatus.ForeColor = Color.Black;
            }
            else
            {
                lblTestStatus.Text = string.Format(Strings.StatusTestingDone, live, dead);
                lblTestStatus.ForeColor = live > 0 ? Color.DarkGreen : Color.DarkSlateGray;
            }
        }

        private void UpdateStrategyLabel()
        {
            string strategyName = Manager.Pool.Strategy?.GetType().Name.Replace("Strategy", "") ?? "Default";
            lblActiveStrategy.Text = string.Format(Strings.StrategyLabel, strategyName);
        }

        #region Event Handlers

        private void chkEnableProxy_CheckedChanged(object sender, EventArgs e)
        {
            Manager.IsEnabled = chkEnableProxy.Checked;
            Manager.SaveToStorage();
        }

        private void cmbProxyType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProxyType.SelectedItem is ProxyType type)
            {
                Manager.DefaultType = type;
                Manager.SaveToStorage();
            }
        }

        private void cmbRotation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbRotation.SelectedItem is RotationMode mode)
            {
                Manager.Rotation = mode;
                switch (mode)
                {
                    case RotationMode.Sticky:
                        Manager.Pool.Strategy = new StickySessionStrategy();
                        break;
                    case RotationMode.Random:
                        Manager.Pool.Strategy = new RandomStrategy();
                        break;
                    case RotationMode.EveryNSeconds:
                    case RotationMode.EveryNRequests:
                    case RotationMode.EveryRequest:
                    case RotationMode.RoundRobin:
                    default:
                        Manager.Pool.Strategy = new RoundRobinStrategy();
                        break;
                }
                UpdateStrategyLabel();
                Manager.SaveToStorage();
            }
        }

        private void numRotateAfter_ValueChanged(object sender, EventArgs e)
        {
            Manager.Pool.RotateAfter = (int)numRotateAfter.Value;
            Manager.SaveToStorage();
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string text = Clipboard.GetText();
                    var parsed = ProxyParser.Parse(text, Manager.DefaultType);
                    if (parsed.Count > 0)
                    {
                        Manager.AddRange(parsed);
                    }
                    else
                    {
                        OnValidationError("Clipboard text did not contain valid proxy format.");
                    }
                }
                else
                {
                    OnValidationError("Clipboard does not contain text.");
                }
            }
            catch (Exception ex)
            {
                OnValidationError($"Failed to paste from clipboard: {ex.Message}");
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = Strings.OpenFileDialogFilter,
                Title = "Import Proxies"
            };

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    string content = File.ReadAllText(dlg.FileName);
                    var parsed = ProxyParser.Parse(content, Manager.DefaultType);
                    if (parsed.Count > 0)
                    {
                        Manager.AddRange(parsed);
                    }
                    else
                    {
                        OnValidationError("Selected file did not contain valid proxy format.");
                    }
                }
                catch (Exception ex)
                {
                    OnValidationError($"Failed to read file: {ex.Message}");
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string host = txtHost.Text.Trim();
            string portStr = txtPort.Text.Trim();

            if (string.IsNullOrEmpty(host))
            {
                OnValidationError("Host IP or Domain cannot be empty.");
                return;
            }

            if (!int.TryParse(portStr, out int port) || port <= 0 || port > 65535)
            {
                OnValidationError("Port must be a valid number between 1 and 65535.");
                return;
            }

            var proxy = new ProxyInfo(host, port, txtUser.Text.Trim(), txtPass.Text.Trim(), Manager.DefaultType);
            Manager.Add(proxy);

            txtHost.Clear();
            txtPort.Clear();
            txtUser.Clear();
            txtPass.Clear();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            var selected = lstProxies.SelectedItems.Cast<ListViewItem>()
                .Select(item => item.Tag as ProxyInfo)
                .Where(p => p != null)
                .ToList();

            foreach (var p in selected)
            {
                if (p != null) Manager.Remove(p);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (Manager.Proxies.Count == 0) return;

            var result = MessageBox.Show(
                "Are you sure you want to clear all proxies?",
                "Clear All Proxies",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Manager.Clear();
            }
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            if (_isTesting) return;

            var targetList = lstProxies.SelectedItems.Count > 0
                ? lstProxies.SelectedItems.Cast<ListViewItem>().Select(i => i.Tag as ProxyInfo).Where(p => p != null).Select(p => p!).ToList()
                : Manager.Proxies.ToList();

            if (targetList.Count == 0)
            {
                OnValidationError("No proxies available to test.");
                return;
            }

            _isTesting = true;
            btnTest.Enabled = false;
            prgHealthCheck.Visible = true;
            prgHealthCheck.Minimum = 0;
            prgHealthCheck.Maximum = targetList.Count;
            prgHealthCheck.Value = 0;

            int testedCount = 0;
            int total = targetList.Count;

            lblTestStatus.Text = string.Format(Strings.StatusTesting, testedCount, total);
            lblTestStatus.ForeColor = Color.Blue;

            var healthChecker = new ProxyHealthChecker(Manager);

            var progress = new Progress<ProxyInfo>(proxy =>
            {
                testedCount++;
                if (prgHealthCheck.Value < prgHealthCheck.Maximum)
                {
                    prgHealthCheck.Value = testedCount;
                }
                lblTestStatus.Text = string.Format(Strings.StatusTesting, testedCount, total);
                UpdateListViewItem(proxy);
                ProxyTested?.Invoke(this, proxy);
            });

            try
            {
                await healthChecker.CheckAllAsync(targetList, maxParallel: 20, progress: progress);
            }
            finally
            {
                _isTesting = false;
                btnTest.Enabled = true;
                prgHealthCheck.Visible = false;
                UpdateStatusLabel();
            }
        }

        private async void btnTestAll_Click(object sender, EventArgs e)
        {
            if (_isTesting) return;

            var targetList = Manager.Proxies.ToList();
            if (targetList.Count == 0)
            {
                OnValidationError("No proxies available in pool to test.");
                return;
            }

            _isTesting = true;
            btnTest.Enabled = false;
            btnTestAll.Enabled = false;
            prgHealthCheck.Visible = true;
            progressBarCheck.Visible = true;

            prgHealthCheck.Minimum = 0;
            prgHealthCheck.Maximum = targetList.Count;
            prgHealthCheck.Value = 0;

            progressBarCheck.Minimum = 0;
            progressBarCheck.Maximum = targetList.Count;
            progressBarCheck.Value = 0;

            int testedCount = 0;
            int total = targetList.Count;

            lblTestStatus.Text = string.Format(Strings.StatusTesting, testedCount, total);
            lblTestStatus.ForeColor = Color.Blue;

            var healthChecker = new ProxyHealthChecker(Manager);

            var progress = new Progress<ProxyInfo>(proxy =>
            {
                testedCount++;
                if (prgHealthCheck.Value < prgHealthCheck.Maximum) prgHealthCheck.Value = testedCount;
                if (progressBarCheck.Value < progressBarCheck.Maximum) progressBarCheck.Value = testedCount;

                lblTestStatus.Text = string.Format(Strings.StatusTesting, testedCount, total);
                UpdateListViewItem(proxy);
                ProxyTested?.Invoke(this, proxy);
            });

            try
            {
                await healthChecker.CheckAllAsync(targetList, maxParallel: 20, progress: progress);
            }
            finally
            {
                _isTesting = false;
                btnTest.Enabled = true;
                btnTestAll.Enabled = true;
                prgHealthCheck.Visible = false;
                progressBarCheck.Visible = false;
                UpdateStatusLabel();
            }
        }

        private async void btnScrapeFree_Click(object sender, EventArgs e)
        {
            if (_isTesting) return;

            _isTesting = true;
            btnScrapeFree.Enabled = false;
            lblTestStatus.Text = "⚡ Harvesting free proxies from public sources...";
            lblTestStatus.ForeColor = Color.Blue;

            try
            {
                var scraper = new FreeProxyScraper();
                var harvested = await scraper.FetchAsync();
                if (harvested.Count > 0)
                {
                    Manager.AddRange(harvested);
                    lblTestStatus.Text = $"Successfully harvested {harvested.Count} free proxies!";
                    lblTestStatus.ForeColor = Color.DarkGreen;
                }
                else
                {
                    OnValidationError("No free proxies were found from active sources.");
                }
            }
            catch (Exception ex)
            {
                OnValidationError($"Scraping failed: {ex.Message}");
            }
            finally
            {
                _isTesting = false;
                btnScrapeFree.Enabled = true;
                UpdateStatusLabel();
            }
        }

        private async void btnDiscoverSources_Click(object sender, EventArgs e)
        {
            if (_isTesting) return;

            _isTesting = true;
            btnDiscoverSources.Enabled = false;
            lblTestStatus.Text = "🔍 Discovering new proxy sources & harvesting proxies...";
            lblTestStatus.ForeColor = Color.Blue;

            try
            {
                var scraper = new FreeProxyScraper();
                int added = await scraper.DiscoverAndAddNewSourcesAsync();
                int totalSources = scraper.GetAllSources().Count;

                var harvested = await scraper.FetchAsync();
                if (harvested.Count > 0)
                {
                    Manager.AddRange(harvested);
                    lblTestStatus.Text = $"Discovered {added} new sources ({totalSources} total) & harvested {harvested.Count} free proxies!";
                    lblTestStatus.ForeColor = Color.DarkGreen;
                }
                else if (added > 0)
                {
                    lblTestStatus.Text = $"Discovered {added} new sources ({totalSources} total), but no proxies were found.";
                    lblTestStatus.ForeColor = Color.Orange;
                }
                else
                {
                    lblTestStatus.Text = $"All {totalSources} sources active, no new proxies harvested.";
                    lblTestStatus.ForeColor = Color.DarkGreen;
                }
            }
            catch (Exception ex)
            {
                OnValidationError($"Source discovery failed: {ex.Message}");
            }
            finally
            {
                _isTesting = false;
                btnDiscoverSources.Enabled = true;
                UpdateStatusLabel();
            }
        }

        private async void btnJudge_Click(object sender, EventArgs e)
        {
            if (_isTesting) return;

            var targetList = lstProxies.SelectedItems.Count > 0
                ? lstProxies.SelectedItems.Cast<ListViewItem>().Select(i => i.Tag as ProxyInfo).Where(p => p != null).Select(p => p!).ToList()
                : Manager.Proxies.Where(p => p.IsLive == true).ToList();

            if (targetList.Count == 0)
            {
                targetList = Manager.Proxies.ToList();
            }

            if (targetList.Count == 0)
            {
                OnValidationError("No proxies available to judge.");
                return;
            }

            _isTesting = true;
            btnJudge.Enabled = false;
            prgHealthCheck.Visible = true;
            prgHealthCheck.Minimum = 0;
            prgHealthCheck.Maximum = targetList.Count;
            prgHealthCheck.Value = 0;

            lblTestStatus.Text = "👑 Judging proxy anonymity & IP leakage...";
            lblTestStatus.ForeColor = Color.Blue;

            var judge = new ProxyJudge();
            int judgedCount = 0;

            try
            {
                foreach (var p in targetList)
                {
                    await judge.JudgeAsync(p);
                    judgedCount++;
                    if (prgHealthCheck.Value < prgHealthCheck.Maximum)
                    {
                        prgHealthCheck.Value = judgedCount;
                    }
                    UpdateListViewItem(p);
                }

                lblTestStatus.Text = $"Judged {judgedCount} proxies successfully!";
                lblTestStatus.ForeColor = Color.DarkGreen;
            }
            catch (Exception ex)
            {
                OnValidationError($"Proxy judging failed: {ex.Message}");
            }
            finally
            {
                _isTesting = false;
                btnJudge.Enabled = true;
                prgHealthCheck.Visible = false;
                UpdateStatusLabel();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Filter = Strings.SaveFileDialogFilter,
                Title = "Save Proxy List"
            };

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    Manager.Save(dlg.FileName);
                    SaveClicked?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    OnValidationError($"Failed to save proxy list: {ex.Message}");
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = Strings.SaveFileDialogFilter,
                Title = "Load Proxy List"
            };

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    Manager.Load(dlg.FileName);
                }
                catch (Exception ex)
                {
                    OnValidationError($"Failed to load proxy list: {ex.Message}");
                }
            }
        }

        #endregion

        private void OnValidationError(string error)
        {
            lblTestStatus.Text = error;
            lblTestStatus.ForeColor = Color.Red;
            ValidationError?.Invoke(this, error);
        }

        /// <summary>
        /// Raises the <see cref="CancelClicked"/> event.
        /// </summary>
        public void OnCancel()
        {
            CancelClicked?.Invoke(this, EventArgs.Empty);
        }

        private void SafeInvoke(Action action)
        {
            if (InvokeRequired)
            {
                BeginInvoke(action);
            }
            else
            {
                action();
            }
        }
    }
}
