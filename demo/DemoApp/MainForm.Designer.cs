namespace DemoApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlTopBar = new System.Windows.Forms.Panel();
            this.lblDemoTitle = new System.Windows.Forms.Label();
            this.btnTestHttpClient = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.proxyControl = new SmartProxyManager.WinForms.ProxyManagerControl();

            this.pnlTopBar.SuspendLayout();
            this.SuspendLayout();

            // pnlTopBar
            this.pnlTopBar.Controls.Add(this.lblDemoTitle);
            this.pnlTopBar.Controls.Add(this.btnTestHttpClient);
            this.pnlTopBar.Controls.Add(this.txtOutput);
            this.pnlTopBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTopBar.Height = 60;
            this.pnlTopBar.Padding = new System.Windows.Forms.Padding(10);

            // lblDemoTitle
            this.lblDemoTitle.AutoSize = true;
            this.lblDemoTitle.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblDemoTitle.Location = new System.Drawing.Point(10, 18);
            this.lblDemoTitle.Name = "lblDemoTitle";
            this.lblDemoTitle.Size = new System.Drawing.Size(202, 20);
            this.lblDemoTitle.TabIndex = 0;
            this.lblDemoTitle.Text = "SmartProxyManager Demo";

            // btnTestHttpClient
            this.btnTestHttpClient.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnTestHttpClient.Location = new System.Drawing.Point(230, 14);
            this.btnTestHttpClient.Name = "btnTestHttpClient";
            this.btnTestHttpClient.Size = new System.Drawing.Size(160, 32);
            this.btnTestHttpClient.TabIndex = 1;
            this.btnTestHttpClient.Text = "Get HttpClient Response";
            this.btnTestHttpClient.UseVisualStyleBackColor = true;
            this.btnTestHttpClient.Click += new System.EventHandler(this.btnTestHttpClient_Click);

            // txtOutput
            this.txtOutput.Location = new System.Drawing.Point(405, 18);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.Size = new System.Drawing.Size(370, 23);
            this.txtOutput.TabIndex = 2;
            this.txtOutput.Text = "Click 'Get HttpClient Response' to test outbound request...";

            // proxyControl
            this.proxyControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.proxyControl.Location = new System.Drawing.Point(0, 60);
            this.proxyControl.Name = "proxyControl";
            this.proxyControl.Size = new System.Drawing.Size(800, 490);
            this.proxyControl.TabIndex = 1;

            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 550);
            this.Controls.Add(this.proxyControl);
            this.Controls.Add(this.pnlTopBar);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SmartProxyManager.WinForms Demo Application";
            this.pnlTopBar.ResumeLayout(false);
            this.pnlTopBar.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlTopBar;
        private System.Windows.Forms.Label lblDemoTitle;
        private System.Windows.Forms.Button btnTestHttpClient;
        private System.Windows.Forms.TextBox txtOutput;
        private SmartProxyManager.WinForms.ProxyManagerControl proxyControl;
    }
}
