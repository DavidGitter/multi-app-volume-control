namespace mavc_target_ui_win
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.SaveBtn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.reverseCheckbox1 = new System.Windows.Forms.CheckBox();
            this.AddVol1 = new System.Windows.Forms.ComboBox();
            this.VolList1 = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.reverseCheckbox2 = new System.Windows.Forms.CheckBox();
            this.AddVol2 = new System.Windows.Forms.ComboBox();
            this.VolList2 = new System.Windows.Forms.ListBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.reverseCheckbox3 = new System.Windows.Forms.CheckBox();
            this.AddVol3 = new System.Windows.Forms.ComboBox();
            this.VolList3 = new System.Windows.Forms.ListBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.reverseCheckbox4 = new System.Windows.Forms.CheckBox();
            this.AddVol4 = new System.Windows.Forms.ComboBox();
            this.VolList4 = new System.Windows.Forms.ListBox();
            this.delItemBtn = new System.Windows.Forms.Button();
            this.discSelBtn = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.darkModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reverseKnobOrderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableDebugModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.minimizeOnCloseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startMinimizedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.overlaySettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.versionLabel = new System.Windows.Forms.Label();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // SaveBtn
            // 
            this.SaveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveBtn.Location = new System.Drawing.Point(845, 3);
            this.SaveBtn.Name = "SaveBtn";
            this.SaveBtn.Size = new System.Drawing.Size(130, 34);
            this.SaveBtn.TabIndex = 6;
            this.SaveBtn.Text = "Save";
            this.SaveBtn.UseVisualStyleBackColor = true;
            this.SaveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.reverseCheckbox1);
            this.groupBox1.Controls.Add(this.AddVol1);
            this.groupBox1.Controls.Add(this.VolList1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(9, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(237, 570);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Volume 1";
            // 
            // reverseCheckbox1
            // 
            this.reverseCheckbox1.AutoSize = true;
            this.reverseCheckbox1.Location = new System.Drawing.Point(6, 21);
            this.reverseCheckbox1.Name = "reverseCheckbox1";
            this.reverseCheckbox1.Size = new System.Drawing.Size(94, 17);
            this.reverseCheckbox1.TabIndex = 0;
            this.reverseCheckbox1.Text = "Reverse Knob";
            this.reverseCheckbox1.UseVisualStyleBackColor = true;
            this.reverseCheckbox1.CheckedChanged += new System.EventHandler(this.reverseCheckbox1_CheckedChanged);
            // 
            // AddVol1
            // 
            this.AddVol1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AddVol1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AddVol1.FormattingEnabled = true;
            this.AddVol1.Location = new System.Drawing.Point(6, 44);
            this.AddVol1.Name = "AddVol1";
            this.AddVol1.Size = new System.Drawing.Size(225, 21);
            this.AddVol1.TabIndex = 1;
            this.AddVol1.SelectedIndexChanged += new System.EventHandler(this.AddVol1_SelectedIndexChanged);
            // 
            // VolList1
            // 
            this.VolList1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.VolList1.FormattingEnabled = true;
            this.VolList1.IntegralHeight = false;
            this.VolList1.Location = new System.Drawing.Point(6, 71);
            this.VolList1.Name = "VolList1";
            this.VolList1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.VolList1.Size = new System.Drawing.Size(225, 493);
            this.VolList1.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.reverseCheckbox2);
            this.groupBox2.Controls.Add(this.AddVol2);
            this.groupBox2.Controls.Add(this.VolList2);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(252, 7);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(237, 570);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Volume 2";
            // 
            // reverseCheckbox2
            // 
            this.reverseCheckbox2.AutoSize = true;
            this.reverseCheckbox2.Location = new System.Drawing.Point(6, 21);
            this.reverseCheckbox2.Name = "reverseCheckbox2";
            this.reverseCheckbox2.Size = new System.Drawing.Size(94, 17);
            this.reverseCheckbox2.TabIndex = 2;
            this.reverseCheckbox2.Text = "Reverse Knob";
            this.reverseCheckbox2.UseVisualStyleBackColor = true;
            this.reverseCheckbox2.CheckedChanged += new System.EventHandler(this.reverseCheckbox2_CheckedChanged);
            // 
            // AddVol2
            // 
            this.AddVol2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AddVol2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AddVol2.FormattingEnabled = true;
            this.AddVol2.Location = new System.Drawing.Point(6, 44);
            this.AddVol2.Name = "AddVol2";
            this.AddVol2.Size = new System.Drawing.Size(225, 21);
            this.AddVol2.TabIndex = 2;
            this.AddVol2.SelectedIndexChanged += new System.EventHandler(this.AddVol2_SelectedIndexChanged);
            // 
            // VolList2
            // 
            this.VolList2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.VolList2.FormattingEnabled = true;
            this.VolList2.IntegralHeight = false;
            this.VolList2.Location = new System.Drawing.Point(6, 71);
            this.VolList2.Name = "VolList2";
            this.VolList2.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.VolList2.Size = new System.Drawing.Size(225, 493);
            this.VolList2.TabIndex = 0;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.reverseCheckbox3);
            this.groupBox4.Controls.Add(this.AddVol3);
            this.groupBox4.Controls.Add(this.VolList3);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(495, 7);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(237, 570);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Volume 3";
            // 
            // reverseCheckbox3
            // 
            this.reverseCheckbox3.AutoSize = true;
            this.reverseCheckbox3.Location = new System.Drawing.Point(6, 21);
            this.reverseCheckbox3.Name = "reverseCheckbox3";
            this.reverseCheckbox3.Size = new System.Drawing.Size(94, 17);
            this.reverseCheckbox3.TabIndex = 9;
            this.reverseCheckbox3.Text = "Reverse Knob";
            this.reverseCheckbox3.UseVisualStyleBackColor = true;
            this.reverseCheckbox3.CheckedChanged += new System.EventHandler(this.reverseCheckbox3_CheckedChanged);
            // 
            // AddVol3
            // 
            this.AddVol3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AddVol3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AddVol3.FormattingEnabled = true;
            this.AddVol3.Location = new System.Drawing.Point(6, 44);
            this.AddVol3.Name = "AddVol3";
            this.AddVol3.Size = new System.Drawing.Size(225, 21);
            this.AddVol3.TabIndex = 1;
            this.AddVol3.SelectedIndexChanged += new System.EventHandler(this.AddVol3_SelectedIndexChanged);
            // 
            // VolList3
            // 
            this.VolList3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.VolList3.FormattingEnabled = true;
            this.VolList3.IntegralHeight = false;
            this.VolList3.Location = new System.Drawing.Point(6, 71);
            this.VolList3.Name = "VolList3";
            this.VolList3.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.VolList3.Size = new System.Drawing.Size(225, 493);
            this.VolList3.TabIndex = 0;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.reverseCheckbox4);
            this.groupBox5.Controls.Add(this.AddVol4);
            this.groupBox5.Controls.Add(this.VolList4);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox5.Location = new System.Drawing.Point(738, 7);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(237, 570);
            this.groupBox5.TabIndex = 9;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Volume 4";
            // 
            // reverseCheckbox4
            // 
            this.reverseCheckbox4.AutoSize = true;
            this.reverseCheckbox4.Location = new System.Drawing.Point(6, 21);
            this.reverseCheckbox4.Name = "reverseCheckbox4";
            this.reverseCheckbox4.Size = new System.Drawing.Size(94, 17);
            this.reverseCheckbox4.TabIndex = 10;
            this.reverseCheckbox4.Text = "Reverse Knob";
            this.reverseCheckbox4.UseVisualStyleBackColor = true;
            this.reverseCheckbox4.CheckedChanged += new System.EventHandler(this.reverseCheckbox4_CheckedChanged);
            // 
            // AddVol4
            // 
            this.AddVol4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AddVol4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AddVol4.FormattingEnabled = true;
            this.AddVol4.Location = new System.Drawing.Point(6, 44);
            this.AddVol4.Name = "AddVol4";
            this.AddVol4.Size = new System.Drawing.Size(225, 21);
            this.AddVol4.TabIndex = 1;
            this.AddVol4.SelectedIndexChanged += new System.EventHandler(this.AddVol4_SelectedIndexChanged);
            // 
            // VolList4
            // 
            this.VolList4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.VolList4.FormattingEnabled = true;
            this.VolList4.IntegralHeight = false;
            this.VolList4.Location = new System.Drawing.Point(6, 71);
            this.VolList4.Name = "VolList4";
            this.VolList4.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.VolList4.Size = new System.Drawing.Size(225, 493);
            this.VolList4.TabIndex = 0;
            // 
            // delItemBtn
            // 
            this.delItemBtn.Location = new System.Drawing.Point(9, 3);
            this.delItemBtn.Name = "delItemBtn";
            this.delItemBtn.Size = new System.Drawing.Size(109, 34);
            this.delItemBtn.TabIndex = 10;
            this.delItemBtn.Text = "Delete Selection";
            this.delItemBtn.UseVisualStyleBackColor = true;
            this.delItemBtn.Click += new System.EventHandler(this.delItemBtn_Click);
            // 
            // discSelBtn
            // 
            this.discSelBtn.Location = new System.Drawing.Point(124, 3);
            this.discSelBtn.Name = "discSelBtn";
            this.discSelBtn.Size = new System.Drawing.Size(109, 34);
            this.discSelBtn.TabIndex = 11;
            this.discSelBtn.Text = "Discard Selection";
            this.discSelBtn.UseVisualStyleBackColor = true;
            this.discSelBtn.Click += new System.EventHandler(this.discSelBtn_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.settingsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(984, 24);
            this.menuStrip1.TabIndex = 12;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(37, 20);
            this.toolStripMenuItem1.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToToolStripMenuItem
            // 
            this.saveToToolStripMenuItem.Name = "saveToToolStripMenuItem";
            this.saveToToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.saveToToolStripMenuItem.Text = "Save To...";
            this.saveToToolStripMenuItem.Click += new System.EventHandler(this.SaveToToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.darkModeToolStripMenuItem,
            this.reverseKnobOrderToolStripMenuItem,
            this.enableDebugModeToolStripMenuItem,
            this.minimizeOnCloseToolStripMenuItem,
            this.startMinimizedToolStripMenuItem,
            this.toolStripSeparator1,
            this.overlaySettingsToolStripMenuItem,
            this.toolStripSeparator2,
            this.refreshToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // darkModeToolStripMenuItem
            // 
            this.darkModeToolStripMenuItem.Name = "darkModeToolStripMenuItem";
            this.darkModeToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this.darkModeToolStripMenuItem.Text = "Dark Mode";
            this.darkModeToolStripMenuItem.Click += new System.EventHandler(this.darkModeToolStripMenuItem_Click);
            // 
            // reverseKnobOrderToolStripMenuItem
            // 
            this.reverseKnobOrderToolStripMenuItem.CheckOnClick = true;
            this.reverseKnobOrderToolStripMenuItem.Name = "reverseKnobOrderToolStripMenuItem";
            this.reverseKnobOrderToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this.reverseKnobOrderToolStripMenuItem.Text = "Reverse Knob Order";
            this.reverseKnobOrderToolStripMenuItem.Click += new System.EventHandler(this.reverseKnobOrderToolStripMenuItem_Click);
            // 
            // enableDebugModeToolStripMenuItem
            // 
            this.enableDebugModeToolStripMenuItem.CheckOnClick = true;
            this.enableDebugModeToolStripMenuItem.Name = "enableDebugModeToolStripMenuItem";
            this.enableDebugModeToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this.enableDebugModeToolStripMenuItem.Text = "Enable Debug Mode (restart agent)";
            this.enableDebugModeToolStripMenuItem.Click += new System.EventHandler(this.enableDebugModeToolStripMenuItem_Click);
            // 
            // minimizeOnCloseToolStripMenuItem
            // 
            this.minimizeOnCloseToolStripMenuItem.CheckOnClick = true;
            this.minimizeOnCloseToolStripMenuItem.Name = "minimizeOnCloseToolStripMenuItem";
            this.minimizeOnCloseToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this.minimizeOnCloseToolStripMenuItem.Text = "Minimize on Close";
            this.minimizeOnCloseToolStripMenuItem.Click += new System.EventHandler(this.minimizeOnCloseToolStripMenuItem_Click);
            // 
            // startMinimizedToolStripMenuItem
            // 
            this.startMinimizedToolStripMenuItem.CheckOnClick = true;
            this.startMinimizedToolStripMenuItem.Name = "startMinimizedToolStripMenuItem";
            this.startMinimizedToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this.startMinimizedToolStripMenuItem.Text = "Start Minimized to Systemtray";
            this.startMinimizedToolStripMenuItem.Click += new System.EventHandler(this.startMinimizedToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(255, 6);
            // 
            // overlaySettingsToolStripMenuItem
            // 
            this.overlaySettingsToolStripMenuItem.Name = "overlaySettingsToolStripMenuItem";
            this.overlaySettingsToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this.overlaySettingsToolStripMenuItem.Text = "Overlay Settings...";
            this.overlaySettingsToolStripMenuItem.Click += new System.EventHandler(this.overlaySettingsToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(255, 6);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this.refreshToolStripMenuItem.Text = "Refresh Audio Outputs";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);
            // 
            // versionLabel
            // 
            this.versionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.versionLabel.Location = new System.Drawing.Point(884, 4);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(88, 16);
            this.versionLabel.TabIndex = 0;
            this.versionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox4, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox5, 3, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(984, 584);
            this.tableLayoutPanel1.TabIndex = 20;
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.delItemBtn);
            this.bottomPanel.Controls.Add(this.discSelBtn);
            this.bottomPanel.Controls.Add(this.SaveBtn);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 608);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Padding = new System.Windows.Forms.Padding(15, 6, 15, 6);
            this.bottomPanel.Size = new System.Drawing.Size(984, 46);
            this.bottomPanel.TabIndex = 21;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 654);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.bottomPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button SaveBtn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox VolList1;
        private System.Windows.Forms.ComboBox AddVol1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox VolList2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ComboBox AddVol3;
        private System.Windows.Forms.ListBox VolList3;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.ComboBox AddVol4;
        private System.Windows.Forms.ListBox VolList4;
        private System.Windows.Forms.Button delItemBtn;
        private System.Windows.Forms.ComboBox AddVol2;
        private System.Windows.Forms.Button discSelBtn;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.CheckBox reverseCheckbox1;
        private System.Windows.Forms.CheckBox reverseCheckbox2;
        private System.Windows.Forms.CheckBox reverseCheckbox3;
        private System.Windows.Forms.CheckBox reverseCheckbox4;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.ToolStripMenuItem darkModeToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reverseKnobOrderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableDebugModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem minimizeOnCloseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startMinimizedToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem overlaySettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}

