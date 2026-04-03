namespace mavc_target_ui_win
{
    partial class KnobSettingsForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KnobSettingsForm));
            this.reverseKnobCheckbox = new System.Windows.Forms.CheckBox();
            this.pinOnController = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // reverseKnobCheckbox
            // 
            this.reverseKnobCheckbox.AutoSize = true;
            this.reverseKnobCheckbox.Location = new System.Drawing.Point(13, 13);
            this.reverseKnobCheckbox.Name = "reverseKnobCheckbox";
            this.reverseKnobCheckbox.Size = new System.Drawing.Size(139, 17);
            this.reverseKnobCheckbox.TabIndex = 0;
            this.reverseKnobCheckbox.Text = "Reverse Knob Direction";
            this.reverseKnobCheckbox.UseVisualStyleBackColor = true;
            this.reverseKnobCheckbox.CheckedChanged += new System.EventHandler(this.reverseKnobCheckbox_CheckedChanged);
            // 
            // pinOnController
            // 
            this.pinOnController.Location = new System.Drawing.Point(13, 36);
            this.pinOnController.Name = "pinOnController";
            this.pinOnController.Size = new System.Drawing.Size(57, 20);
            this.pinOnController.TabIndex = 1;
            this.pinOnController.TextChanged += new System.EventHandler(this.pinOnController_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(76, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Controller-Pin (currently unsupported)";
            // 
            // KnobSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(307, 181);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pinOnController);
            this.Controls.Add(this.reverseKnobCheckbox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "KnobSettingsForm";
            this.Text = "KnobSettingsForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox reverseKnobCheckbox;
        private System.Windows.Forms.TextBox pinOnController;
        private System.Windows.Forms.Label label1;
    }
}