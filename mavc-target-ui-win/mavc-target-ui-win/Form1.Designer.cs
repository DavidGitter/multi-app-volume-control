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
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.listBox3 = new System.Windows.Forms.ListBox();
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
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.darkModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.additionalBox = new System.Windows.Forms.GroupBox();
            this.startMinimized = new System.Windows.Forms.CheckBox();
            this.closeActionToggle = new System.Windows.Forms.CheckBox();
            this.enableDebugBox = new System.Windows.Forms.CheckBox();
            this.reverseKnobsCheckbox = new System.Windows.Forms.CheckBox();
            this.versionText = new System.Windows.Forms.TextBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.enableScreenOverlayCheckbox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.additionalBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // SaveBtn
            // 
            this.SaveBtn.Location = new System.Drawing.Point(1072, 654);
            this.SaveBtn.Name = "SaveBtn";
            this.SaveBtn.Size = new System.Drawing.Size(146, 38);
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
            this.groupBox1.Location = new System.Drawing.Point(12, 38);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(234, 608);
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
            this.AddVol1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AddVol1.FormattingEnabled = true;
            this.AddVol1.Location = new System.Drawing.Point(6, 44);
            this.AddVol1.Name = "AddVol1";
            this.AddVol1.Size = new System.Drawing.Size(222, 21);
            this.AddVol1.TabIndex = 1;
            this.AddVol1.SelectedIndexChanged += new System.EventHandler(this.AddVol1_SelectedIndexChanged);
            // 
            // VolList1
            // 
            this.VolList1.FormattingEnabled = true;
            this.VolList1.Location = new System.Drawing.Point(0, 71);
            this.VolList1.Name = "VolList1";
            this.VolList1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.VolList1.Size = new System.Drawing.Size(234, 537);
            this.VolList1.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.reverseCheckbox2);
            this.groupBox2.Controls.Add(this.AddVol2);
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.VolList2);
            this.groupBox2.Location = new System.Drawing.Point(252, 38);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(234, 608);
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
            this.AddVol2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AddVol2.FormattingEnabled = true;
            this.AddVol2.Location = new System.Drawing.Point(6, 44);
            this.AddVol2.Name = "AddVol2";
            this.AddVol2.Size = new System.Drawing.Size(222, 21);
            this.AddVol2.TabIndex = 2;
            this.AddVol2.SelectedIndexChanged += new System.EventHandler(this.AddVol2_SelectedIndexChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.comboBox3);
            this.groupBox3.Controls.Add(this.listBox3);
            this.groupBox3.Location = new System.Drawing.Point(240, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(234, 614);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "groupBox3";
            // 
            // comboBox3
            // 
            this.comboBox3.FormattingEnabled = true;
            this.comboBox3.Location = new System.Drawing.Point(0, 44);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(234, 21);
            this.comboBox3.TabIndex = 1;
            // 
            // listBox3
            // 
            this.listBox3.FormattingEnabled = true;
            this.listBox3.Location = new System.Drawing.Point(0, 71);
            this.listBox3.Name = "listBox3";
            this.listBox3.Size = new System.Drawing.Size(234, 537);
            this.listBox3.TabIndex = 0;
            // 
            // VolList2
            // 
            this.VolList2.FormattingEnabled = true;
            this.VolList2.Location = new System.Drawing.Point(0, 71);
            this.VolList2.Name = "VolList2";
            this.VolList2.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.VolList2.Size = new System.Drawing.Size(234, 537);
            this.VolList2.TabIndex = 0;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.reverseCheckbox3);
            this.groupBox4.Controls.Add(this.AddVol3);
            this.groupBox4.Controls.Add(this.VolList3);
            this.groupBox4.Location = new System.Drawing.Point(492, 38);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(234, 608);
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
            this.AddVol3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AddVol3.FormattingEnabled = true;
            this.AddVol3.Location = new System.Drawing.Point(6, 44);
            this.AddVol3.Name = "AddVol3";
            this.AddVol3.Size = new System.Drawing.Size(222, 21);
            this.AddVol3.TabIndex = 1;
            this.AddVol3.SelectedIndexChanged += new System.EventHandler(this.AddVol3_SelectedIndexChanged);
            // 
            // VolList3
            // 
            this.VolList3.FormattingEnabled = true;
            this.VolList3.Location = new System.Drawing.Point(0, 71);
            this.VolList3.Name = "VolList3";
            this.VolList3.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.VolList3.Size = new System.Drawing.Size(234, 537);
            this.VolList3.TabIndex = 0;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.reverseCheckbox4);
            this.groupBox5.Controls.Add(this.AddVol4);
            this.groupBox5.Controls.Add(this.VolList4);
            this.groupBox5.Location = new System.Drawing.Point(732, 38);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(234, 608);
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
            this.AddVol4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AddVol4.FormattingEnabled = true;
            this.AddVol4.Location = new System.Drawing.Point(6, 44);
            this.AddVol4.Name = "AddVol4";
            this.AddVol4.Size = new System.Drawing.Size(222, 21);
            this.AddVol4.TabIndex = 1;
            this.AddVol4.SelectedIndexChanged += new System.EventHandler(this.AddVol4_SelectedIndexChanged);
            // 
            // VolList4
            // 
            this.VolList4.FormattingEnabled = true;
            this.VolList4.Location = new System.Drawing.Point(0, 71);
            this.VolList4.Name = "VolList4";
            this.VolList4.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.VolList4.Size = new System.Drawing.Size(234, 537);
            this.VolList4.TabIndex = 0;
            // 
            // delItemBtn
            // 
            this.delItemBtn.Location = new System.Drawing.Point(12, 652);
            this.delItemBtn.Name = "delItemBtn";
            this.delItemBtn.Size = new System.Drawing.Size(146, 38);
            this.delItemBtn.TabIndex = 10;
            this.delItemBtn.Text = "Delete Selection";
            this.delItemBtn.UseVisualStyleBackColor = true;
            this.delItemBtn.Click += new System.EventHandler(this.delItemBtn_Click);
            // 
            // discSelBtn
            // 
            this.discSelBtn.Location = new System.Drawing.Point(164, 652);
            this.discSelBtn.Name = "discSelBtn";
            this.discSelBtn.Size = new System.Drawing.Size(146, 38);
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
            this.helpToolStripMenuItem,
            this.refreshToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1231, 24);
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
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.darkModeToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // darkModeToolStripMenuItem
            // 
            this.darkModeToolStripMenuItem.Name = "darkModeToolStripMenuItem";
            this.darkModeToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.darkModeToolStripMenuItem.Text = "Dark Mode";
            this.darkModeToolStripMenuItem.Click += new System.EventHandler(this.darkModeToolStripMenuItem_Click);
            // 
            // additionalBox
            // 
            this.additionalBox.Controls.Add(this.enableScreenOverlayCheckbox);
            this.additionalBox.Controls.Add(this.startMinimized);
            this.additionalBox.Controls.Add(this.closeActionToggle);
            this.additionalBox.Controls.Add(this.enableDebugBox);
            this.additionalBox.Controls.Add(this.reverseKnobsCheckbox);
            this.additionalBox.Location = new System.Drawing.Point(972, 38);
            this.additionalBox.Name = "additionalBox";
            this.additionalBox.Size = new System.Drawing.Size(246, 608);
            this.additionalBox.TabIndex = 13;
            this.additionalBox.TabStop = false;
            this.additionalBox.Text = "Settings";
            //
            // startMinimized
            // 
            this.startMinimized.AutoSize = true;
            this.startMinimized.Location = new System.Drawing.Point(6, 88);
            this.startMinimized.Name = "startMinimizedBox";
            this.startMinimized.Size = new System.Drawing.Size(162, 17);
            this.startMinimized.TabIndex = 14;
            this.startMinimized.Text = "Start minimized to Systemtray";
            this.startMinimized.UseVisualStyleBackColor = true;
            this.startMinimized.CheckedChanged += new System.EventHandler(this.startMinimized_CheckedChanged);
            // 
            // closeActionToggle
            // 
            this.closeActionToggle.AutoSize = true;
            this.closeActionToggle.Location = new System.Drawing.Point(6, 65);
            this.closeActionToggle.Name = "closeActionToggleBox";
            this.closeActionToggle.Size = new System.Drawing.Size(109, 17);
            this.closeActionToggle.TabIndex = 13;
            this.closeActionToggle.Text = "Minimize on close";
            this.closeActionToggle.UseVisualStyleBackColor = true;
            this.closeActionToggle.CheckedChanged += new System.EventHandler(this.closeActionToggle_CheckedChanged);
            // 
            // enableDebugBox
            // 
            this.enableDebugBox.AutoSize = true;
            this.enableDebugBox.Location = new System.Drawing.Point(6, 42);
            this.enableDebugBox.Name = "enableDebugBox";
            this.enableDebugBox.Size = new System.Drawing.Size(221, 17);
            this.enableDebugBox.TabIndex = 12;
            this.enableDebugBox.Text = "Enable debug mode (agent needs restart)";
            this.enableDebugBox.UseVisualStyleBackColor = true;
            this.enableDebugBox.CheckedChanged += new System.EventHandler(this.enableDebugBox_CheckedChanged);
            // 
            // reverseKnobsCheckbox
            // 
            this.reverseKnobsCheckbox.AutoSize = true;
            this.reverseKnobsCheckbox.Location = new System.Drawing.Point(6, 19);
            this.reverseKnobsCheckbox.Name = "reverseKnobsCheckbox";
            this.reverseKnobsCheckbox.Size = new System.Drawing.Size(120, 17);
            this.reverseKnobsCheckbox.TabIndex = 11;
            this.reverseKnobsCheckbox.Text = "Reverse knob order";
            this.reverseKnobsCheckbox.UseVisualStyleBackColor = true;
            this.reverseKnobsCheckbox.CheckedChanged += new System.EventHandler(this.reverseKnobsCheckbox_CheckedChanged);
            // 
            // versionText
            // 
            this.versionText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.versionText.BackColor = System.Drawing.SystemColors.MenuBar;
            this.versionText.Location = new System.Drawing.Point(1159, 4);
            this.versionText.Name = "versionText";
            this.versionText.ReadOnly = true;
            this.versionText.Size = new System.Drawing.Size(72, 20);
            this.versionText.TabIndex = 12;
            this.versionText.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // enableScreenOverlayCheckbox
            // 
            this.enableScreenOverlayCheckbox.AutoSize = true;
            this.enableScreenOverlayCheckbox.Location = new System.Drawing.Point(6, 88);
            this.enableScreenOverlayCheckbox.Name = "enableScreenOverlayCheckbox";
            this.enableScreenOverlayCheckbox.Size = new System.Drawing.Size(141, 17);
            this.enableScreenOverlayCheckbox.TabIndex = 14;
            this.enableScreenOverlayCheckbox.Text = "Activate Screen Overlay";
            this.enableScreenOverlayCheckbox.UseVisualStyleBackColor = true;
            this.enableScreenOverlayCheckbox.CheckedChanged += new System.EventHandler(this.enableScreenOverlayCheckbox_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1231, 704);
            this.Controls.Add(this.versionText);
            this.Controls.Add(this.additionalBox);
            this.Controls.Add(this.discSelBtn);
            this.Controls.Add(this.delItemBtn);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.SaveBtn);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.additionalBox.ResumeLayout(false);
            this.additionalBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button SaveBtn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox VolList1;
        private System.Windows.Forms.ComboBox AddVol1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.ListBox listBox3;
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
        private System.Windows.Forms.GroupBox additionalBox;
        private System.Windows.Forms.CheckBox reverseCheckbox1;
        private System.Windows.Forms.CheckBox reverseCheckbox2;
        private System.Windows.Forms.CheckBox reverseCheckbox3;
        private System.Windows.Forms.CheckBox reverseCheckbox4;
        private System.Windows.Forms.CheckBox reverseKnobsCheckbox;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.TextBox versionText;
        private System.Windows.Forms.CheckBox closeActionToggle;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem darkModeToolStripMenuItem;
        private System.Windows.Forms.CheckBox enableDebugBox;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.CheckBox enableScreenOverlayCheckbox;
        private System.Windows.Forms.CheckBox startMinimized;
    }
}

