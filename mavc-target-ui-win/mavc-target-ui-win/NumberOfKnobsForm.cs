using System.Drawing;
using System.Windows.Forms;

namespace mavc_target_ui_win
{
    /**
     * Dialog that lets the user choose the number of hardware knobs (1–16).
     *
     * Extracted from the inline Form creation in Form1 so that the layout,
     * theming, and result property are defined in one dedicated place.
     */
    public class NumberOfKnobsForm : Form
    {
        private readonly NumericUpDown _nud;

        /**
         * The knob count selected by the user.
         * Only valid after ShowDialog returns DialogResult.OK.
         */
        public int SelectedValue => (int)_nud.Value;

        /**
         * Creates and lays out the dialog.
         *
         * @param currentValue  the currently configured knob count shown as default
         * @param darkMode      true to apply the dark theme, false for light
         */
        public NumberOfKnobsForm(int currentValue, bool darkMode)
        {
            Text = "Number of Knobs";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(260, 100);

            var lbl = new Label
            {
                Text = "Number of knobs (1-16):",
                Location = new Point(12, 15),
                AutoSize = true
            };

            _nud = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 16,
                Value = currentValue,
                Location = new Point(180, 12),
                Size = new Size(60, 20)
            };

            var btnOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(85, 55),
                Size = new Size(90, 30),
                TabStop = false
            };

            AcceptButton = btnOk;
            Controls.Add(lbl);
            Controls.Add(_nud);
            Controls.Add(btnOk);

            if (darkMode)
                ThemeColors.ApplyTheme(this, darkMode);
        }
    }
}
