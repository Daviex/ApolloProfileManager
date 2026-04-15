namespace ApolloProfileManager;

partial class ClientManagerDialog
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        splitContainer1    = new System.Windows.Forms.SplitContainer();
        lstClients         = new System.Windows.Forms.ListBox();
        lblClientName      = new System.Windows.Forms.Label();
        lblClientUuid      = new System.Windows.Forms.Label();
        lblClientRun       = new System.Windows.Forms.Label();
        lblClientSave      = new System.Windows.Forms.Label();
        btnOpenDir         = new System.Windows.Forms.Button();
        btnDeleteClient    = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
        splitContainer1.Panel1.SuspendLayout();
        splitContainer1.Panel2.SuspendLayout();
        splitContainer1.SuspendLayout();
        SuspendLayout();
        //
        // splitContainer1.Panel1
        //
        splitContainer1.Panel1.Controls.Add(lstClients);
        //
        // splitContainer1.Panel2
        //
        splitContainer1.Panel2.Controls.Add(lblClientName);
        splitContainer1.Panel2.Controls.Add(lblClientUuid);
        splitContainer1.Panel2.Controls.Add(lblClientRun);
        splitContainer1.Panel2.Controls.Add(lblClientSave);
        splitContainer1.Panel2.Controls.Add(btnOpenDir);
        splitContainer1.Panel2.Controls.Add(btnDeleteClient);
        //
        // splitContainer1
        //
        splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
        splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
        splitContainer1.Location = new System.Drawing.Point(0, 0);
        splitContainer1.Name = "splitContainer1";
        splitContainer1.Panel1MinSize = 150;
        splitContainer1.Panel2MinSize = 190;
        splitContainer1.Size = new System.Drawing.Size(584, 261);
        splitContainer1.SplitterDistance = 380;
        splitContainer1.TabIndex = 0;
        //
        // lstClients
        //
        lstClients.Dock = System.Windows.Forms.DockStyle.Fill;
        lstClients.FormattingEnabled = true;
        lstClients.Location = new System.Drawing.Point(0, 0);
        lstClients.Name = "lstClients";
        lstClients.Size = new System.Drawing.Size(380, 261);
        lstClients.TabIndex = 0;
        lstClients.SelectedIndexChanged += LstClients_SelectedIndexChanged;
        //
        // lblClientName
        //
        lblClientName.AutoSize = true;
        lblClientName.Location = new System.Drawing.Point(5, 5);
        lblClientName.Name = "lblClientName";
        lblClientName.Size = new System.Drawing.Size(65, 15);
        lblClientName.TabIndex = 0;
        lblClientName.Text = "Name: N/A";
        //
        // lblClientUuid
        //
        lblClientUuid.AutoSize = true;
        lblClientUuid.Location = new System.Drawing.Point(5, 28);
        lblClientUuid.Name = "lblClientUuid";
        lblClientUuid.Size = new System.Drawing.Size(55, 15);
        lblClientUuid.TabIndex = 1;
        lblClientUuid.Text = "UUID: {\u2014}";
        //
        // lblClientRun
        //
        lblClientRun.AutoSize = true;
        lblClientRun.Location = new System.Drawing.Point(5, 51);
        lblClientRun.Name = "lblClientRun";
        lblClientRun.Size = new System.Drawing.Size(72, 15);
        lblClientRun.TabIndex = 2;
        lblClientRun.Text = "Last run: \u2014";
        //
        // lblClientSave
        //
        lblClientSave.AutoSize = true;
        lblClientSave.Location = new System.Drawing.Point(5, 74);
        lblClientSave.Name = "lblClientSave";
        lblClientSave.Size = new System.Drawing.Size(77, 15);
        lblClientSave.TabIndex = 3;
        lblClientSave.Text = "Last save: \u2014";
        //
        // btnOpenDir
        //
        btnOpenDir.Enabled = false;
        btnOpenDir.Location = new System.Drawing.Point(5, 105);
        btnOpenDir.Name = "btnOpenDir";
        btnOpenDir.Size = new System.Drawing.Size(185, 28);
        btnOpenDir.TabIndex = 4;
        btnOpenDir.Text = "Open dir";
        btnOpenDir.UseVisualStyleBackColor = true;
        btnOpenDir.Click += BtnOpenDir_Click;
        //
        // btnDeleteClient
        //
        btnDeleteClient.Enabled = false;
        btnDeleteClient.Location = new System.Drawing.Point(5, 138);
        btnDeleteClient.Name = "btnDeleteClient";
        btnDeleteClient.Size = new System.Drawing.Size(185, 28);
        btnDeleteClient.TabIndex = 5;
        btnDeleteClient.Text = "Delete client";
        btnDeleteClient.UseVisualStyleBackColor = true;
        btnDeleteClient.Click += BtnDeleteClient_Click;
        //
        // ClientManagerDialog
        //
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(584, 261);
        Controls.Add(splitContainer1);
        MinimumSize = new System.Drawing.Size(400, 200);
        Name = "ClientManagerDialog";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        splitContainer1.Panel1.ResumeLayout(false);
        splitContainer1.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
        splitContainer1.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.ListBox lstClients;
    private System.Windows.Forms.Label lblClientName;
    private System.Windows.Forms.Label lblClientUuid;
    private System.Windows.Forms.Label lblClientRun;
    private System.Windows.Forms.Label lblClientSave;
    private System.Windows.Forms.Button btnOpenDir;
    private System.Windows.Forms.Button btnDeleteClient;
}
