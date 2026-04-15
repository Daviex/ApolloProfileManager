namespace ApolloProfileManager;

partial class ProfileManagerGUI
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
        splitContainer1 = new System.Windows.Forms.SplitContainer();
        lstGames = new System.Windows.Forms.ListBox();
        lblName = new System.Windows.Forms.Label();
        lblUuid = new System.Windows.Forms.Label();
        lblLastRun = new System.Windows.Forms.Label();
        lblLastSave = new System.Windows.Forms.Label();
        btnEdit = new System.Windows.Forms.Button();
        btnManage = new System.Windows.Forms.Button();
        btnOpen = new System.Windows.Forms.Button();
        btnDelete = new System.Windows.Forms.Button();
        btnInject = new System.Windows.Forms.Button();
        btnConfig = new System.Windows.Forms.Button();
        btnProfilesDir = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
        splitContainer1.Panel1.SuspendLayout();
        splitContainer1.Panel2.SuspendLayout();
        splitContainer1.SuspendLayout();
        SuspendLayout();
        //
        // splitContainer1.Panel1
        //
        splitContainer1.Panel1.Controls.Add(lstGames);
        //
        // splitContainer1.Panel2
        //
        splitContainer1.Panel2.Controls.Add(lblName);
        splitContainer1.Panel2.Controls.Add(lblUuid);
        splitContainer1.Panel2.Controls.Add(lblLastRun);
        splitContainer1.Panel2.Controls.Add(lblLastSave);
        splitContainer1.Panel2.Controls.Add(btnEdit);
        splitContainer1.Panel2.Controls.Add(btnManage);
        splitContainer1.Panel2.Controls.Add(btnOpen);
        splitContainer1.Panel2.Controls.Add(btnDelete);
        splitContainer1.Panel2.Controls.Add(btnProfilesDir);
        splitContainer1.Panel2.Controls.Add(btnInject);
        splitContainer1.Panel2.Controls.Add(btnConfig);
        //
        // splitContainer1
        //
        splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
        splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
        splitContainer1.Location = new System.Drawing.Point(0, 0);
        splitContainer1.Name = "splitContainer1";
        splitContainer1.Panel1MinSize = 200;
        splitContainer1.Panel2MinSize = 215;
        splitContainer1.Size = new System.Drawing.Size(784, 361);
        splitContainer1.SplitterDistance = 560;
        splitContainer1.TabIndex = 0;
        //
        // lstGames
        //
        lstGames.Dock = System.Windows.Forms.DockStyle.Fill;
        lstGames.FormattingEnabled = true;
        lstGames.Location = new System.Drawing.Point(0, 0);
        lstGames.Name = "lstGames";
        lstGames.Size = new System.Drawing.Size(560, 361);
        lstGames.TabIndex = 0;
        lstGames.SelectedIndexChanged += LstGames_SelectedIndexChanged;
        lstGames.DoubleClick += LstGames_DoubleClick;
        //
        // lblName
        //
        lblName.AutoSize = true;
        lblName.Location = new System.Drawing.Point(5, 5);
        lblName.Name = "lblName";
        lblName.Size = new System.Drawing.Size(55, 15);
        lblName.TabIndex = 0;
        lblName.Text = "Name: \u2014";
        //
        // lblUuid
        //
        lblUuid.AutoSize = true;
        lblUuid.Location = new System.Drawing.Point(5, 28);
        lblUuid.Name = "lblUuid";
        lblUuid.Size = new System.Drawing.Size(44, 15);
        lblUuid.TabIndex = 1;
        lblUuid.Text = "UUID: \u2014";
        //
        // lblLastRun
        //
        lblLastRun.AutoSize = true;
        lblLastRun.Location = new System.Drawing.Point(5, 51);
        lblLastRun.Name = "lblLastRun";
        lblLastRun.Size = new System.Drawing.Size(72, 15);
        lblLastRun.TabIndex = 2;
        lblLastRun.Text = "Last run: \u2014";
        //
        // lblLastSave
        //
        lblLastSave.AutoSize = true;
        lblLastSave.Location = new System.Drawing.Point(5, 74);
        lblLastSave.Name = "lblLastSave";
        lblLastSave.Size = new System.Drawing.Size(77, 15);
        lblLastSave.TabIndex = 3;
        lblLastSave.Text = "Last save: \u2014";
        //
        // btnEdit
        //
        btnEdit.Enabled = false;
        btnEdit.Location = new System.Drawing.Point(5, 105);
        btnEdit.Name = "btnEdit";
        btnEdit.Size = new System.Drawing.Size(205, 28);
        btnEdit.TabIndex = 4;
        btnEdit.Text = "Edit Tracked Files";
        btnEdit.UseVisualStyleBackColor = true;
        btnEdit.Click += BtnEdit_Click;
        //
        // btnManage
        //
        btnManage.Enabled = false;
        btnManage.Location = new System.Drawing.Point(5, 138);
        btnManage.Name = "btnManage";
        btnManage.Size = new System.Drawing.Size(205, 28);
        btnManage.TabIndex = 5;
        btnManage.Text = "Manage Client Saves";
        btnManage.UseVisualStyleBackColor = true;
        btnManage.Click += BtnManage_Click;
        //
        // btnOpen
        //
        btnOpen.Enabled = false;
        btnOpen.Location = new System.Drawing.Point(5, 171);
        btnOpen.Name = "btnOpen";
        btnOpen.Size = new System.Drawing.Size(205, 28);
        btnOpen.TabIndex = 6;
        btnOpen.Text = "Open Profile Dir";
        btnOpen.UseVisualStyleBackColor = true;
        btnOpen.Click += BtnOpen_Click;
        //
        // btnDelete
        //
        btnDelete.Enabled = false;
        btnDelete.Location = new System.Drawing.Point(5, 204);
        btnDelete.Name = "btnDelete";
        btnDelete.Size = new System.Drawing.Size(205, 28);
        btnDelete.TabIndex = 7;
        btnDelete.Text = "Delete App Profile";
        btnDelete.UseVisualStyleBackColor = true;
        btnDelete.Click += BtnDelete_Click;
        //
        // btnProfilesDir  (anchored to bottom)
        //
        btnProfilesDir.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Bottom;
        btnProfilesDir.Location = new System.Drawing.Point(5, 262);
        btnProfilesDir.Name = "btnProfilesDir";
        btnProfilesDir.Size = new System.Drawing.Size(205, 28);
        btnProfilesDir.TabIndex = 10;
        btnProfilesDir.Text = "Change Profiles Directory";
        btnProfilesDir.UseVisualStyleBackColor = true;
        btnProfilesDir.Click += BtnProfilesDir_Click;
        //
        // btnInject  (anchored to bottom)
        //
        btnInject.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Bottom;
        btnInject.Location = new System.Drawing.Point(5, 295);
        btnInject.Name = "btnInject";
        btnInject.Size = new System.Drawing.Size(205, 28);
        btnInject.TabIndex = 8;
        btnInject.Text = "Inject Global Prep Commands";
        btnInject.UseVisualStyleBackColor = true;
        btnInject.Click += BtnInject_Click;
        //
        // btnConfig  (anchored to bottom)
        //
        btnConfig.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Bottom;
        btnConfig.Location = new System.Drawing.Point(5, 328);
        btnConfig.Name = "btnConfig";
        btnConfig.Size = new System.Drawing.Size(205, 28);
        btnConfig.TabIndex = 9;
        btnConfig.Text = "Change Apollo Config File";
        btnConfig.UseVisualStyleBackColor = true;
        btnConfig.Click += BtnConfig_Click;
        //
        // ProfileManagerGUI
        //
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(784, 361);
        Controls.Add(splitContainer1);
        MinimumSize = new System.Drawing.Size(600, 300);
        Name = "ProfileManagerGUI";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "Apollo Profile Manager";
        splitContainer1.Panel1.ResumeLayout(false);
        splitContainer1.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
        splitContainer1.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.ListBox lstGames;
    private System.Windows.Forms.Label lblName;
    private System.Windows.Forms.Label lblUuid;
    private System.Windows.Forms.Label lblLastRun;
    private System.Windows.Forms.Label lblLastSave;
    private System.Windows.Forms.Button btnEdit;
    private System.Windows.Forms.Button btnManage;
    private System.Windows.Forms.Button btnOpen;
    private System.Windows.Forms.Button btnDelete;
    private System.Windows.Forms.Button btnInject;
    private System.Windows.Forms.Button btnConfig;
    private System.Windows.Forms.Button btnProfilesDir;
}
