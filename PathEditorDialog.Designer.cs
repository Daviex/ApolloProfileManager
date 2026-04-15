namespace ApolloProfileManager;

partial class PathEditorDialog
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
        grpPaths    = new System.Windows.Forms.GroupBox();
        lstPaths    = new System.Windows.Forms.ListBox();
        pnlButtons  = new System.Windows.Forms.Panel();
        btnAddDir   = new System.Windows.Forms.Button();
        btnAddFile  = new System.Windows.Forms.Button();
        btnRemove   = new System.Windows.Forms.Button();
        btnClose    = new System.Windows.Forms.Button();
        grpPaths.SuspendLayout();
        pnlButtons.SuspendLayout();
        SuspendLayout();
        //
        // grpPaths
        //
        grpPaths.Controls.Add(lstPaths);
        grpPaths.Dock = System.Windows.Forms.DockStyle.Fill;
        grpPaths.Location = new System.Drawing.Point(0, 0);
        grpPaths.Name = "grpPaths";
        grpPaths.Padding = new System.Windows.Forms.Padding(5);
        grpPaths.Size = new System.Drawing.Size(484, 216);
        grpPaths.TabIndex = 0;
        grpPaths.TabStop = false;
        grpPaths.Text = "Tracked paths";
        //
        // lstPaths
        //
        lstPaths.AllowDrop = true;
        lstPaths.Dock = System.Windows.Forms.DockStyle.Fill;
        lstPaths.FormattingEnabled = true;
        lstPaths.Location = new System.Drawing.Point(5, 19);
        lstPaths.Name = "lstPaths";
        lstPaths.Size = new System.Drawing.Size(474, 192);
        lstPaths.TabIndex = 0;
        lstPaths.DragEnter += LstPaths_DragEnter;
        lstPaths.DragDrop  += LstPaths_DragDrop;
        //
        // pnlButtons
        //
        pnlButtons.Controls.Add(btnAddDir);
        pnlButtons.Controls.Add(btnAddFile);
        pnlButtons.Controls.Add(btnRemove);
        pnlButtons.Controls.Add(btnClose);
        pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
        pnlButtons.Location = new System.Drawing.Point(0, 216);
        pnlButtons.Name = "pnlButtons";
        pnlButtons.Size = new System.Drawing.Size(484, 45);
        pnlButtons.TabIndex = 1;
        //
        // btnAddDir
        //
        btnAddDir.Location = new System.Drawing.Point(5, 8);
        btnAddDir.Name = "btnAddDir";
        btnAddDir.Size = new System.Drawing.Size(80, 28);
        btnAddDir.TabIndex = 0;
        btnAddDir.Text = "Add dir";
        btnAddDir.UseVisualStyleBackColor = true;
        btnAddDir.Click += BtnAddDir_Click;
        //
        // btnAddFile
        //
        btnAddFile.Location = new System.Drawing.Point(90, 8);
        btnAddFile.Name = "btnAddFile";
        btnAddFile.Size = new System.Drawing.Size(80, 28);
        btnAddFile.TabIndex = 1;
        btnAddFile.Text = "Add file";
        btnAddFile.UseVisualStyleBackColor = true;
        btnAddFile.Click += BtnAddFile_Click;
        //
        // btnRemove
        //
        btnRemove.Location = new System.Drawing.Point(175, 8);
        btnRemove.Name = "btnRemove";
        btnRemove.Size = new System.Drawing.Size(80, 28);
        btnRemove.TabIndex = 2;
        btnRemove.Text = "Remove";
        btnRemove.UseVisualStyleBackColor = true;
        btnRemove.Click += BtnRemove_Click;
        //
        // btnClose
        //
        btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        btnClose.Location = new System.Drawing.Point(399, 8);
        btnClose.Name = "btnClose";
        btnClose.Size = new System.Drawing.Size(80, 28);
        btnClose.TabIndex = 3;
        btnClose.Text = "Close";
        btnClose.UseVisualStyleBackColor = true;
        btnClose.Click += BtnClose_Click;
        //
        // PathEditorDialog
        //
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(484, 261);
        Controls.Add(grpPaths);
        Controls.Add(pnlButtons);
        MinimumSize = new System.Drawing.Size(350, 200);
        Name = "PathEditorDialog";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        grpPaths.ResumeLayout(false);
        pnlButtons.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.GroupBox grpPaths;
    private System.Windows.Forms.ListBox lstPaths;
    private System.Windows.Forms.Panel pnlButtons;
    private System.Windows.Forms.Button btnAddDir;
    private System.Windows.Forms.Button btnAddFile;
    private System.Windows.Forms.Button btnRemove;
    private System.Windows.Forms.Button btnClose;
}
