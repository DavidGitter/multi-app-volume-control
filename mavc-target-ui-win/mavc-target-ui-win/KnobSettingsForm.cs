using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mavc_target_ui_win
{
    public partial class KnobSettingsForm : Form
    {
        MAVCSave mavcSave;
        int knobIndex;

        public KnobSettingsForm(int knobIndex, MAVCSave mavcSave, bool darkMode)
        {
            InitializeComponent();
            this.Text = "Settings Knob " + knobIndex+1;
            this.KeyPreview = true;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;     
            this.MinimizeBox = false;  
            this.ControlBox = true;

            // configure window size
            this.StartPosition = FormStartPosition.Manual; // sehr wichtig!
            this.Size = new Size(300, 200); // Beispielgröße

            // set window to cursor position
            Point mousePos = Cursor.Position; // global screen coords
            this.Location = new Point(mousePos.X, mousePos.Y);

            this.mavcSave = mavcSave;
            this.knobIndex = knobIndex;

            //Initialize Confs
            this.reverseKnobCheckbox.Checked = mavcSave.reverseKnobs[knobIndex];
            this.pinOnController.Text = mavcSave.pinMappings[knobIndex].ToString();

            var btnOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(85, 70),
                Size = new Size(90, 30),
                TabStop = false
            };

            AcceptButton = btnOk;
            Controls.Add(btnOk);

            if (darkMode)
                ThemeColors.ApplyTheme(this, darkMode);
        }

        private void reverseKnobCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            this.mavcSave.reverseKnobs[knobIndex] = reverseKnobCheckbox.Checked;
        }

        private void pinOnController_TextChanged(object sender, EventArgs e)
        {
            this.mavcSave.pinMappings[knobIndex] = int.Parse(pinOnController.Text);
        }
    }
}
