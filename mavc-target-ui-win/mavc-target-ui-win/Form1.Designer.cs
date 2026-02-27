namespace mavc_target_ui_win
{
    partial class Form1
    {
        /**
         * Required designer variable for component initialization.
         */
        private System.ComponentModel.IContainer components = null;

        /**
         * Clean up any resources being used.
         * @param disposing True if managed resources should be disposed; otherwise False.
         */
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /**
         * Required method for designer support.
         * Do not modify the contents of this method with the code editor.
         */
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.SaveBtn = new System.Windows.Forms.Button();
            this.delItemBtn = new System.Windows.Forms.Button();
            this.discSelBtn = new System.Windows.Forms.Button();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.darkModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reverseKnobOrderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableDebugModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.minimizeOnCloseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startMinimizedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.numberOfKnobsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.overlaySettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.refreshVolumesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.versionLabel = new System.Windows.Forms.Label();
            this.buttonPanel.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // SaveBtn
            // 
            this.SaveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveBtn.Location = new System.Drawing.Point(787, 0);
            this.SaveBtn.Name = "SaveBtn";
            this.SaveBtn.Size = new System.Drawing.Size(103, 26);
            this.SaveBtn.TabIndex = 0;
            this.SaveBtn.Text = "Save";
            this.SaveBtn.UseVisualStyleBackColor = true;
            this.SaveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // delItemBtn
            // 
            this.delItemBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.delItemBtn.Location = new System.Drawing.Point(10, 0);
            this.delItemBtn.Name = "delItemBtn";
            this.delItemBtn.Size = new System.Drawing.Size(103, 26);
            this.delItemBtn.TabIndex = 1;
            this.delItemBtn.Text = "Delete Selection";
            this.delItemBtn.UseVisualStyleBackColor = true;
            this.delItemBtn.Click += new System.EventHandler(this.delItemBtn_Click);
            // 
            // discSelBtn
            // 
            this.discSelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.discSelBtn.Location = new System.Drawing.Point(119, 0);
            this.discSelBtn.Name = "discSelBtn";
            this.discSelBtn.Size = new System.Drawing.Size(103, 26);
            this.discSelBtn.TabIndex = 2;
            this.discSelBtn.Text = "Discard Selection";
            this.discSelBtn.UseVisualStyleBackColor = true;
            this.discSelBtn.Click += new System.EventHandler(this.discSelBtn_Click);
            // 
            // buttonPanel
            // 
            this.buttonPanel.Controls.Add(this.SaveBtn);
            this.buttonPanel.Controls.Add(this.discSelBtn);
            this.buttonPanel.Controls.Add(this.delItemBtn);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = new System.Drawing.Point(0, 430);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Padding = new System.Windows.Forms.Padding(7, 0, 7, 7);
            this.buttonPanel.Size = new System.Drawing.Size(900, 33);
            this.buttonPanel.TabIndex = 5;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(7);
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(900, 406);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(900, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.SaveToToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.openToolStripMenuItem.Text = "Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // SaveToToolStripMenuItem
            // 
            this.SaveToToolStripMenuItem.Name = "SaveToToolStripMenuItem";
            this.SaveToToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.SaveToToolStripMenuItem.Text = "Save To...";
            this.SaveToToolStripMenuItem.Click += new System.EventHandler(this.SaveToToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.darkModeToolStripMenuItem,
            this.toolStripSeparator2,
            this.reverseKnobOrderToolStripMenuItem,
            this.enableDebugModeToolStripMenuItem,
            this.minimizeOnCloseToolStripMenuItem,
            this.startMinimizedToolStripMenuItem,
            this.toolStripSeparator3,
            this.numberOfKnobsToolStripMenuItem,
            this.overlaySettingsToolStripMenuItem,
            this.toolStripSeparator4,
            this.refreshVolumesToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(179, 6);
            // 
            // darkModeToolStripMenuItem
            // 
            this.darkModeToolStripMenuItem.Name = "darkModeToolStripMenuItem";
            this.darkModeToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.darkModeToolStripMenuItem.Text = "Dark Mode";
            this.darkModeToolStripMenuItem.Click += new System.EventHandler(this.darkModeToolStripMenuItem_Click);
            // 
            // reverseKnobOrderToolStripMenuItem
            // 
            this.reverseKnobOrderToolStripMenuItem.Name = "reverseKnobOrderToolStripMenuItem";
            this.reverseKnobOrderToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.reverseKnobOrderToolStripMenuItem.Text = "Reverse Knob Order";
            this.reverseKnobOrderToolStripMenuItem.Click += new System.EventHandler(this.reverseKnobOrderToolStripMenuItem_Click);
            // 
            // enableDebugModeToolStripMenuItem
            // 
            this.enableDebugModeToolStripMenuItem.Name = "enableDebugModeToolStripMenuItem";
            this.enableDebugModeToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.enableDebugModeToolStripMenuItem.Text = "Enable Debug Mode";
            this.enableDebugModeToolStripMenuItem.Click += new System.EventHandler(this.enableDebugModeToolStripMenuItem_Click);
            // 
            // minimizeOnCloseToolStripMenuItem
            // 
            this.minimizeOnCloseToolStripMenuItem.Name = "minimizeOnCloseToolStripMenuItem";
            this.minimizeOnCloseToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.minimizeOnCloseToolStripMenuItem.Text = "Minimize on Close";
            this.minimizeOnCloseToolStripMenuItem.Click += new System.EventHandler(this.minimizeOnCloseToolStripMenuItem_Click);
            // 
            // startMinimizedToolStripMenuItem
            // 
            this.startMinimizedToolStripMenuItem.Name = "startMinimizedToolStripMenuItem";
            this.startMinimizedToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.startMinimizedToolStripMenuItem.Text = "Start Minimized";
            this.startMinimizedToolStripMenuItem.Click += new System.EventHandler(this.startMinimizedToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(179, 6);
            // 
            // numberOfKnobsToolStripMenuItem
            // 
            this.numberOfKnobsToolStripMenuItem.Name = "numberOfKnobsToolStripMenuItem";
            this.numberOfKnobsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.numberOfKnobsToolStripMenuItem.Text = "Number of Knobs...";
            this.numberOfKnobsToolStripMenuItem.Click += new System.EventHandler(this.numberOfKnobsToolStripMenuItem_Click);
            // 
            // overlaySettingsToolStripMenuItem
            // 
            this.overlaySettingsToolStripMenuItem.Name = "overlaySettingsToolStripMenuItem";
            this.overlaySettingsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.overlaySettingsToolStripMenuItem.Text = "Overlay Settings...";
            this.overlaySettingsToolStripMenuItem.Click += new System.EventHandler(this.overlaySettingsToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(179, 6);
            // 
            // refreshVolumesToolStripMenuItem
            // 
            this.refreshVolumesToolStripMenuItem.Name = "refreshVolumesToolStripMenuItem";
            this.refreshVolumesToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.refreshVolumesToolStripMenuItem.Text = "Refresh Volume Lists";
            this.refreshVolumesToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // versionLabel
            // 
            this.versionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(860, 2);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(28, 13);
            this.versionLabel.TabIndex = 6;
            this.versionLabel.Text = "v1.0";
            this.versionLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 463);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.buttonPanel);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "MAVC";
            this.buttonPanel.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button SaveBtn;
        private System.Windows.Forms.Button delItemBtn;
        private System.Windows.Forms.Button discSelBtn;
        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SaveToToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem darkModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reverseKnobOrderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableDebugModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem minimizeOnCloseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startMinimizedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem numberOfKnobsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem overlaySettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem refreshVolumesToolStripMenuItem;
        private System.Windows.Forms.Label versionLabel;
    }
}
