namespace ProxyForge.WinForms
{
    partial class ProxyManagerControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlTop = new System.Windows.Forms.Panel();
            this.chkEnableProxy = new System.Windows.Forms.CheckBox();
            this.lblProxyType = new System.Windows.Forms.Label();
            this.cmbProxyType = new System.Windows.Forms.ComboBox();
            this.lblRotation = new System.Windows.Forms.Label();
            this.cmbRotation = new System.Windows.Forms.ComboBox();
            this.lblRotateAfter = new System.Windows.Forms.Label();
            this.numRotateAfter = new System.Windows.Forms.NumericUpDown();
            this.lblActiveStrategy = new System.Windows.Forms.Label();
            this.lblCurrentProxy = new System.Windows.Forms.Label();
            this.lblSearch = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();

            this.pnlMain = new System.Windows.Forms.TableLayoutPanel();
            this.lstProxies = new System.Windows.Forms.ListView();
            this.colHost = new System.Windows.Forms.ColumnHeader();
            this.colPort = new System.Windows.Forms.ColumnHeader();
            this.colUsername = new System.Windows.Forms.ColumnHeader();
            this.colCountry = new System.Windows.Forms.ColumnHeader();
            this.colAnonymity = new System.Windows.Forms.ColumnHeader();
            this.colStatus = new System.Windows.Forms.ColumnHeader();
            this.colLatency = new System.Windows.Forms.ColumnHeader();
            this.colType = new System.Windows.Forms.ColumnHeader();
            this.colUsage = new System.Windows.Forms.ColumnHeader();

            this.pnlActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnPaste = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnScrapeFree = new System.Windows.Forms.Button();
            this.btnDiscoverSources = new System.Windows.Forms.Button();
            this.btnJudge = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnTestAll = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();

            this.grpAddSingle = new System.Windows.Forms.GroupBox();
            this.pnlSingleAddLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.lblHost = new System.Windows.Forms.Label();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lblUser = new System.Windows.Forms.Label();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.lblPass = new System.Windows.Forms.Label();
            this.txtPass = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();

            this.pnlStatus = new System.Windows.Forms.Panel();
            this.lblTestStatus = new System.Windows.Forms.Label();
            this.prgHealthCheck = new System.Windows.Forms.ProgressBar();
            this.progressBarCheck = new System.Windows.Forms.ProgressBar();

            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRotateAfter)).BeginInit();
            this.pnlMain.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.grpAddSingle.SuspendLayout();
            this.pnlSingleAddLayout.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            this.SuspendLayout();

            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.chkEnableProxy);
            this.pnlTop.Controls.Add(this.lblProxyType);
            this.pnlTop.Controls.Add(this.cmbProxyType);
            this.pnlTop.Controls.Add(this.lblRotation);
            this.pnlTop.Controls.Add(this.cmbRotation);
            this.pnlTop.Controls.Add(this.lblRotateAfter);
            this.pnlTop.Controls.Add(this.numRotateAfter);
            this.pnlTop.Controls.Add(this.lblActiveStrategy);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Height = 45;
            this.pnlTop.Padding = new System.Windows.Forms.Padding(10, 10, 10, 5);

            // chkEnableProxy
            this.chkEnableProxy.AutoSize = true;
            this.chkEnableProxy.Checked = true;
            this.chkEnableProxy.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableProxy.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.chkEnableProxy.Location = new System.Drawing.Point(10, 12);
            this.chkEnableProxy.Name = "chkEnableProxy";
            this.chkEnableProxy.Size = new System.Drawing.Size(95, 19);
            this.chkEnableProxy.TabIndex = 0;
            this.chkEnableProxy.Text = Resources.Strings.EnableProxy;
            this.chkEnableProxy.UseVisualStyleBackColor = true;
            this.chkEnableProxy.CheckedChanged += new System.EventHandler(this.chkEnableProxy_CheckedChanged);

            // lblProxyType
            this.lblProxyType.AutoSize = true;
            this.lblProxyType.Location = new System.Drawing.Point(115, 14);
            this.lblProxyType.Name = "lblProxyType";
            this.lblProxyType.Size = new System.Drawing.Size(34, 15);
            this.lblProxyType.TabIndex = 1;
            this.lblProxyType.Text = Resources.Strings.Type;

            // cmbProxyType
            this.cmbProxyType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProxyType.FormattingEnabled = true;
            this.cmbProxyType.Location = new System.Drawing.Point(152, 11);
            this.cmbProxyType.Name = "cmbProxyType";
            this.cmbProxyType.Size = new System.Drawing.Size(80, 23);
            this.cmbProxyType.TabIndex = 2;
            this.cmbProxyType.SelectedIndexChanged += new System.EventHandler(this.cmbProxyType_SelectedIndexChanged);

            // lblRotation
            this.lblRotation.AutoSize = true;
            this.lblRotation.Location = new System.Drawing.Point(240, 14);
            this.lblRotation.Name = "lblRotation";
            this.lblRotation.Size = new System.Drawing.Size(55, 15);
            this.lblRotation.TabIndex = 3;
            this.lblRotation.Text = Resources.Strings.Rotation;

            // cmbRotation
            this.cmbRotation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRotation.FormattingEnabled = true;
            this.cmbRotation.Location = new System.Drawing.Point(298, 11);
            this.cmbRotation.Name = "cmbRotation";
            this.cmbRotation.Size = new System.Drawing.Size(125, 23);
            this.cmbRotation.TabIndex = 4;
            this.cmbRotation.SelectedIndexChanged += new System.EventHandler(this.cmbRotation_SelectedIndexChanged);

            // lblRotateAfter
            this.lblRotateAfter.AutoSize = true;
            this.lblRotateAfter.Location = new System.Drawing.Point(430, 14);
            this.lblRotateAfter.Name = "lblRotateAfter";
            this.lblRotateAfter.Size = new System.Drawing.Size(73, 15);
            this.lblRotateAfter.TabIndex = 5;
            this.lblRotateAfter.Text = Resources.Strings.RotateAfterLabel;

            // numRotateAfter
            this.numRotateAfter.Location = new System.Drawing.Point(506, 11);
            this.numRotateAfter.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numRotateAfter.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numRotateAfter.Name = "numRotateAfter";
            this.numRotateAfter.Size = new System.Drawing.Size(55, 23);
            this.numRotateAfter.TabIndex = 6;
            this.numRotateAfter.Value = new decimal(new int[] { 10, 0, 0, 0 });
            this.numRotateAfter.ValueChanged += new System.EventHandler(this.numRotateAfter_ValueChanged);

            // lblActiveStrategy
            this.lblActiveStrategy.AutoSize = true;
            this.lblActiveStrategy.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblActiveStrategy.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblActiveStrategy.Location = new System.Drawing.Point(575, 14);
            this.lblActiveStrategy.Name = "lblActiveStrategy";
            this.lblActiveStrategy.Size = new System.Drawing.Size(150, 15);
            this.lblActiveStrategy.TabIndex = 7;
            this.lblActiveStrategy.Text = "Strategy: RoundRobin";

            // 
            // pnlMain
            // 
            this.pnlMain.ColumnCount = 2;
            this.pnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.pnlMain.Controls.Add(this.lstProxies, 0, 0);
            this.pnlMain.Controls.Add(this.pnlActions, 1, 0);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 45);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.RowCount = 1;
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlMain.Size = new System.Drawing.Size(760, 320);

            // lstProxies
            this.lstProxies.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colHost,
            this.colPort,
            this.colUsername,
            this.colCountry,
            this.colAnonymity,
            this.colStatus,
            this.colLatency,
            this.colType,
            this.colUsage});
            this.lstProxies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstProxies.FullRowSelect = true;
            this.lstProxies.GridLines = true;
            this.lstProxies.Location = new System.Drawing.Point(10, 5);
            this.lstProxies.Margin = new System.Windows.Forms.Padding(10, 5, 5, 5);
            this.lstProxies.Name = "lstProxies";
            this.lstProxies.Size = new System.Drawing.Size(615, 310);
            this.lstProxies.TabIndex = 0;
            this.lstProxies.UseCompatibleStateImageBehavior = false;
            this.lstProxies.View = System.Windows.Forms.View.Details;

            this.colHost.Text = Resources.Strings.ColHost;
            this.colHost.Width = 120;
            this.colPort.Text = Resources.Strings.ColPort;
            this.colPort.Width = 55;
            this.colUsername.Text = Resources.Strings.ColUsername;
            this.colUsername.Width = 90;
            this.colCountry.Text = "Country";
            this.colCountry.Width = 65;
            this.colAnonymity.Text = "Anonymity";
            this.colAnonymity.Width = 85;
            this.colStatus.Text = Resources.Strings.ColStatus;
            this.colStatus.Width = 90;
            this.colLatency.Text = Resources.Strings.ColLatency;
            this.colLatency.Width = 70;
            this.colType.Text = Resources.Strings.ColType;
            this.colType.Width = 65;
            this.colUsage.Text = "Usage";
            this.colUsage.Width = 65;

            // pnlActions
            this.pnlActions.Controls.Add(this.btnPaste);
            this.pnlActions.Controls.Add(this.btnImport);
            this.pnlActions.Controls.Add(this.btnScrapeFree);
            this.pnlActions.Controls.Add(this.btnJudge);
            this.pnlActions.Controls.Add(this.btnTest);
            this.pnlActions.Controls.Add(this.btnTestAll);
            this.pnlActions.Controls.Add(this.btnRemove);
            this.pnlActions.Controls.Add(this.btnClear);
            this.pnlActions.Controls.Add(this.btnSave);
            this.pnlActions.Controls.Add(this.btnLoad);
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlActions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlActions.Location = new System.Drawing.Point(630, 5);
            this.pnlActions.Margin = new System.Windows.Forms.Padding(0, 5, 10, 5);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Size = new System.Drawing.Size(120, 310);
            this.pnlActions.TabIndex = 1;

            this.btnPaste.Location = new System.Drawing.Point(3, 3);
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.Size = new System.Drawing.Size(114, 25);
            this.btnPaste.TabIndex = 0;
            this.btnPaste.Text = Resources.Strings.PasteFromClipboard;
            this.btnPaste.UseVisualStyleBackColor = true;
            this.btnPaste.Click += new System.EventHandler(this.btnPaste_Click);

            this.btnImport.Location = new System.Drawing.Point(3, 31);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(114, 25);
            this.btnImport.TabIndex = 1;
            this.btnImport.Text = Resources.Strings.ImportTxt;
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);

            this.btnScrapeFree.Location = new System.Drawing.Point(3, 59);
            this.btnScrapeFree.Name = "btnScrapeFree";
            this.btnScrapeFree.Size = new System.Drawing.Size(114, 25);
            this.btnScrapeFree.TabIndex = 8;
            this.btnScrapeFree.Text = "⚡ Free Proxies";
            this.btnScrapeFree.UseVisualStyleBackColor = true;
            this.btnScrapeFree.Click += new System.EventHandler(this.btnScrapeFree_Click);

            this.btnDiscoverSources.Location = new System.Drawing.Point(3, 87);
            this.btnDiscoverSources.Name = "btnDiscoverSources";
            this.btnDiscoverSources.Size = new System.Drawing.Size(114, 25);
            this.btnDiscoverSources.TabIndex = 9;
            this.btnDiscoverSources.Text = "🔍 Discover";
            this.btnDiscoverSources.UseVisualStyleBackColor = true;
            this.btnDiscoverSources.Click += new System.EventHandler(this.btnDiscoverSources_Click);

            this.btnJudge.Location = new System.Drawing.Point(3, 115);
            this.btnJudge.Name = "btnJudge";
            this.btnJudge.Size = new System.Drawing.Size(114, 25);
            this.btnJudge.TabIndex = 10;
            this.btnJudge.Text = "👑 Judge";
            this.btnJudge.UseVisualStyleBackColor = true;
            this.btnJudge.Click += new System.EventHandler(this.btnJudge_Click);

            this.btnTest.Location = new System.Drawing.Point(3, 75);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(114, 30);
            this.btnTest.TabIndex = 2;
            this.btnTest.Text = Resources.Strings.TestSelected;
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);

            this.btnTestAll.Location = new System.Drawing.Point(3, 111);
            this.btnTestAll.Name = "btnTestAll";
            this.btnTestAll.Size = new System.Drawing.Size(114, 30);
            this.btnTestAll.TabIndex = 7;
            this.btnTestAll.Text = "Test All";
            this.btnTestAll.UseVisualStyleBackColor = true;
            this.btnTestAll.Click += new System.EventHandler(this.btnTestAll_Click);

            this.btnRemove.Location = new System.Drawing.Point(3, 111);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(114, 30);
            this.btnRemove.TabIndex = 3;
            this.btnRemove.Text = Resources.Strings.RemoveSelected;
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);

            this.btnClear.Location = new System.Drawing.Point(3, 147);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(114, 30);
            this.btnClear.TabIndex = 4;
            this.btnClear.Text = Resources.Strings.ClearAll;
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);

            this.btnSave.Location = new System.Drawing.Point(3, 183);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(114, 30);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = Resources.Strings.SaveList;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            this.btnLoad.Location = new System.Drawing.Point(3, 219);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(114, 30);
            this.btnLoad.TabIndex = 6;
            this.btnLoad.Text = Resources.Strings.LoadList;
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);

            // grpAddSingle
            this.grpAddSingle.Controls.Add(this.pnlSingleAddLayout);
            this.grpAddSingle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.grpAddSingle.Height = 65;
            this.grpAddSingle.Location = new System.Drawing.Point(0, 365);
            this.grpAddSingle.Margin = new System.Windows.Forms.Padding(10, 0, 10, 5);
            this.grpAddSingle.Name = "grpAddSingle";
            this.grpAddSingle.Size = new System.Drawing.Size(760, 65);
            this.grpAddSingle.TabIndex = 1;
            this.grpAddSingle.TabStop = false;
            this.grpAddSingle.Text = Resources.Strings.SingleProxyHeader;

            // pnlSingleAddLayout
            this.pnlSingleAddLayout.Controls.Add(this.lblHost);
            this.pnlSingleAddLayout.Controls.Add(this.txtHost);
            this.pnlSingleAddLayout.Controls.Add(this.lblPort);
            this.pnlSingleAddLayout.Controls.Add(this.txtPort);
            this.pnlSingleAddLayout.Controls.Add(this.lblUser);
            this.pnlSingleAddLayout.Controls.Add(this.txtUser);
            this.pnlSingleAddLayout.Controls.Add(this.lblPass);
            this.pnlSingleAddLayout.Controls.Add(this.txtPass);
            this.pnlSingleAddLayout.Controls.Add(this.btnAdd);
            this.pnlSingleAddLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSingleAddLayout.Location = new System.Drawing.Point(3, 19);
            this.pnlSingleAddLayout.Name = "pnlSingleAddLayout";
            this.pnlSingleAddLayout.Padding = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.pnlSingleAddLayout.Size = new System.Drawing.Size(754, 43);

            this.lblHost.AutoSize = true;
            this.lblHost.Location = new System.Drawing.Point(8, 8);
            this.lblHost.Name = "lblHost";
            this.lblHost.Size = new System.Drawing.Size(35, 15);
            this.lblHost.Text = Resources.Strings.HostLabel;

            this.txtHost.Location = new System.Drawing.Point(49, 5);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(120, 23);
            this.txtHost.TabIndex = 0;

            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(175, 8);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(32, 15);
            this.lblPort.Text = Resources.Strings.PortLabel;

            this.txtPort.Location = new System.Drawing.Point(210, 5);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(55, 23);
            this.txtPort.TabIndex = 1;

            this.lblUser.AutoSize = true;
            this.lblUser.Location = new System.Drawing.Point(271, 8);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(33, 15);
            this.lblUser.Text = Resources.Strings.UsernameLabel;

            this.txtUser.Location = new System.Drawing.Point(307, 5);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(90, 23);
            this.txtUser.TabIndex = 2;

            this.lblPass.AutoSize = true;
            this.lblPass.Location = new System.Drawing.Point(403, 8);
            this.lblPass.Name = "lblPass";
            this.lblPass.Size = new System.Drawing.Size(30, 15);
            this.lblPass.Text = Resources.Strings.PasswordLabel;

            this.txtPass.Location = new System.Drawing.Point(436, 5);
            this.txtPass.Name = "txtPass";
            this.txtPass.PasswordChar = '*';
            this.txtPass.Size = new System.Drawing.Size(90, 23);
            this.txtPass.TabIndex = 3;

            this.btnAdd.Location = new System.Drawing.Point(532, 4);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(90, 25);
            this.btnAdd.TabIndex = 4;
            this.btnAdd.Text = Resources.Strings.AddProxy;
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);

            // pnlStatus
            this.pnlStatus.Controls.Add(this.lblCurrentProxy);
            this.pnlStatus.Controls.Add(this.lblTestStatus);
            this.pnlStatus.Controls.Add(this.prgHealthCheck);
            this.pnlStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatus.Height = 28;
            this.pnlStatus.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);

            // lblCurrentProxy
            this.lblCurrentProxy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrentProxy.AutoSize = true;
            this.lblCurrentProxy.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblCurrentProxy.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblCurrentProxy.Location = new System.Drawing.Point(350, 5);
            this.lblCurrentProxy.Name = "lblCurrentProxy";
            this.lblCurrentProxy.Size = new System.Drawing.Size(85, 15);
            this.lblCurrentProxy.TabIndex = 2;
            this.lblCurrentProxy.Text = "Current: None";

            // lblTestStatus
            this.lblTestStatus.AutoSize = true;
            this.lblTestStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblTestStatus.Location = new System.Drawing.Point(10, 5);
            this.lblTestStatus.Name = "lblTestStatus";
            this.lblTestStatus.Size = new System.Drawing.Size(74, 15);
            this.lblTestStatus.TabIndex = 0;
            this.lblTestStatus.Text = Resources.Strings.StatusReady;

            // prgHealthCheck
            this.prgHealthCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.prgHealthCheck.Location = new System.Drawing.Point(550, 5);
            this.prgHealthCheck.Name = "prgHealthCheck";
            this.prgHealthCheck.Size = new System.Drawing.Size(200, 18);
            this.prgHealthCheck.TabIndex = 1;
            this.prgHealthCheck.Visible = false;

            // ProxyManagerControl
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.grpAddSingle);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.pnlStatus);
            this.Name = "ProxyManagerControl";
            this.Size = new System.Drawing.Size(760, 458);
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRotateAfter)).EndInit();
            this.pnlMain.ResumeLayout(false);
            this.pnlActions.ResumeLayout(false);
            this.grpAddSingle.ResumeLayout(false);
            this.pnlSingleAddLayout.ResumeLayout(false);
            this.pnlSingleAddLayout.PerformLayout();
            this.pnlStatus.ResumeLayout(false);
            this.pnlStatus.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.CheckBox chkEnableProxy;
        private System.Windows.Forms.Label lblProxyType;
        private System.Windows.Forms.ComboBox cmbProxyType;
        private System.Windows.Forms.Label lblRotation;
        private System.Windows.Forms.ComboBox cmbRotation;
        private System.Windows.Forms.Label lblRotateAfter;
        private System.Windows.Forms.NumericUpDown numRotateAfter;
        private System.Windows.Forms.Label lblActiveStrategy;
        private System.Windows.Forms.Label lblCurrentProxy;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;

        private System.Windows.Forms.TableLayoutPanel pnlMain;
        private System.Windows.Forms.ListView lstProxies;
        private System.Windows.Forms.ColumnHeader colHost;
        private System.Windows.Forms.ColumnHeader colPort;
        private System.Windows.Forms.ColumnHeader colUsername;
        private System.Windows.Forms.ColumnHeader colCountry;
        private System.Windows.Forms.ColumnHeader colAnonymity;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ColumnHeader colLatency;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colUsage;

        private System.Windows.Forms.FlowLayoutPanel pnlActions;
        private System.Windows.Forms.Button btnPaste;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnScrapeFree;
        private System.Windows.Forms.Button btnDiscoverSources;
        private System.Windows.Forms.Button btnJudge;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button btnTestAll;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnLoad;

        private System.Windows.Forms.GroupBox grpAddSingle;
        private System.Windows.Forms.FlowLayoutPanel pnlSingleAddLayout;
        private System.Windows.Forms.Label lblHost;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Label lblPass;
        private System.Windows.Forms.TextBox txtPass;
        private System.Windows.Forms.Button btnAdd;

        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.Label lblTestStatus;
        private System.Windows.Forms.ProgressBar prgHealthCheck;
        private System.Windows.Forms.ProgressBar progressBarCheck;
    }
}
