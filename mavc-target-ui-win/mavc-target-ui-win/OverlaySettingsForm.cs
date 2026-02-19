using System;
using System.Drawing;
using System.Windows.Forms;

namespace mavc_target_ui_win
{
    /**
     * A small settings dialog that consolidates every overlay-related option:
     * enable/disable toggle, auto-hide with delay, X/Y position sliders
     * (0 … primary-monitor resolution), and a "Save & Restart Overlay" button.
     *
     * Slider changes are throttled and live-saved so the overlay moves in real time.
     */
    public class OverlaySettingsForm : Form
    {
        // ?? controls ??
        private CheckBox chkEnabled;
        private CheckBox chkAutoHide;
        private Label lblDelay;
        private TextBox txtDelay;
        private Label lblX;
        private TrackBar sliderX;
        private TextBox txtX;
        private Label lblY;
        private TrackBar sliderY;
        private TextBox txtY;
        private Button btnSaveRestart;

        // ?? state ??
        private readonly MAVCSave _save;
        private readonly Action _onSaveAndRestart;
        private readonly Action _onLiveSave;
        private readonly bool _isDark;

        // ?? throttled live-save timer ??
        private readonly Timer _liveTimer = new Timer();
        private bool _dirty = false;

        /**
         * Creates the overlay-settings dialog.
         *
         * @param save              the shared config object to read from / write to
         * @param isDark            true to apply the dark theme immediately
         * @param onSaveAndRestart  callback invoked when the user clicks "Save & Restart Overlay"
         * @param onLiveSave        callback invoked on throttled slider changes (persist without restart)
         */
        public OverlaySettingsForm(MAVCSave save, bool isDark, Action onSaveAndRestart, Action onLiveSave)
        {
            _save = save;
            _isDark = isDark;
            _onSaveAndRestart = onSaveAndRestart;
            _onLiveSave = onLiveSave;

            _liveTimer.Interval = 30;
            _liveTimer.Tick += (s, e) =>
            {
                if (!_dirty) return;
                _dirty = false;
                _save.overlayX = sliderX.Value;
                _save.overlayY = sliderY.Value;
                _onLiveSave?.Invoke();
            };

            BuildUI();
            LoadFromSave();

            if (isDark)
                ThemeColors.ApplyTheme(this, true);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _liveTimer.Stop();
            _liveTimer.Dispose();
            base.OnFormClosed(e);
        }

        // ?????????????????????? layout ??????????????????????

        private void BuildUI()
        {
            Text = "Overlay Settings";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            ClientSize = new Size(420, 300);

            int screenW = Screen.PrimaryScreen.Bounds.Width;
            int screenH = Screen.PrimaryScreen.Bounds.Height;

            int y = 14;
            int lblLeft = 14;
            int ctrlLeft = 150;
            int ctrlWidth = 254;

            // ?? Enable Overlay ??
            chkEnabled = new CheckBox
            {
                Text = "Enable Overlay",
                Location = new Point(lblLeft, y),
                AutoSize = true
            };
            Controls.Add(chkEnabled);
            y += 30;

            // ?? Auto-Hide ??
            chkAutoHide = new CheckBox
            {
                Text = "Auto-Hide",
                Location = new Point(lblLeft, y),
                AutoSize = true
            };
            Controls.Add(chkAutoHide);
            y += 30;

            // ?? Hide-after delay ??
            lblDelay = new Label
            {
                Text = "Hide after (sec):",
                Location = new Point(lblLeft, y + 3),
                AutoSize = true
            };
            Controls.Add(lblDelay);

            txtDelay = new TextBox
            {
                Location = new Point(ctrlLeft, y),
                Width = 60
            };
            Controls.Add(txtDelay);
            y += 34;

            // ?? X Position ??
            lblX = new Label
            {
                Text = "X Position:",
                Location = new Point(lblLeft, y + 3),
                AutoSize = true
            };
            Controls.Add(lblX);

            sliderX = new TrackBar
            {
                Minimum = 0,
                Maximum = screenW,
                TickFrequency = screenW / 10,
                SmallChange = 1,
                LargeChange = screenW / 20,
                Location = new Point(ctrlLeft, y),
                Width = ctrlWidth - 70
            };
            sliderX.ValueChanged += Slider_ValueChanged;
            Controls.Add(sliderX);

            txtX = new TextBox
            {
                Location = new Point(ctrlLeft + ctrlWidth - 60, y + 4),
                Width = 60
            };
            txtX.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
                {
                    int val;
                    if (int.TryParse(txtX.Text, out val))
                        sliderX.Value = Math.Max(sliderX.Minimum, Math.Min(sliderX.Maximum, val));
                    e.SuppressKeyPress = true;
                }
            };
            Controls.Add(txtX);
            y += 50;

            // ?? Y Position ??
            lblY = new Label
            {
                Text = "Y Position:",
                Location = new Point(lblLeft, y + 3),
                AutoSize = true
            };
            Controls.Add(lblY);

            sliderY = new TrackBar
            {
                Minimum = 0,
                Maximum = screenH,
                TickFrequency = screenH / 10,
                SmallChange = 1,
                LargeChange = screenH / 20,
                Location = new Point(ctrlLeft, y),
                Width = ctrlWidth - 70
            };
            sliderY.ValueChanged += Slider_ValueChanged;
            Controls.Add(sliderY);

            txtY = new TextBox
            {
                Location = new Point(ctrlLeft + ctrlWidth - 60, y + 4),
                Width = 60
            };
            txtY.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
                {
                    int val;
                    if (int.TryParse(txtY.Text, out val))
                        sliderY.Value = Math.Max(sliderY.Minimum, Math.Min(sliderY.Maximum, val));
                    e.SuppressKeyPress = true;
                }
            };
            Controls.Add(txtY);
            y += 56;

            // ?? Save & Restart Overlay ??
            btnSaveRestart = new Button
            {
                Text = "Save && Restart Overlay",
                Location = new Point(lblLeft, y),
                Size = new Size(ctrlLeft + ctrlWidth - lblLeft, 34)
            };
            btnSaveRestart.Click += BtnSaveRestart_Click;
            Controls.Add(btnSaveRestart);

            // size the form to fit contents
            ClientSize = new Size(420, y + btnSaveRestart.Height + 14);
        }

        // ?????????????????????? live position update ??????????????????????

        private void Slider_ValueChanged(object sender, EventArgs e)
        {
            txtX.Text = sliderX.Value.ToString();
            txtY.Text = sliderY.Value.ToString();
            _dirty = true;
            if (!_liveTimer.Enabled) _liveTimer.Start();
        }

        // ?????????????????????? data binding ??????????????????????

        private void LoadFromSave()
        {
            chkEnabled.Checked = _save.enableScreenOverlay;
            chkAutoHide.Checked = _save.activateAutoHide;
            txtDelay.Text = _save.autoHideAfterSec.ToString();
            sliderX.Value = Math.Max(sliderX.Minimum, Math.Min(sliderX.Maximum, _save.overlayX));
            sliderY.Value = Math.Max(sliderY.Minimum, Math.Min(sliderY.Maximum, _save.overlayY));
            txtX.Text = _save.overlayX.ToString();
            txtY.Text = _save.overlayY.ToString();
        }

        private void WriteToSave()
        {
            _save.enableScreenOverlay = chkEnabled.Checked;
            _save.activateAutoHide = chkAutoHide.Checked;

            int delay;
            if (int.TryParse(txtDelay.Text, out delay))
                _save.autoHideAfterSec = delay;

            _save.overlayX = sliderX.Value;
            _save.overlayY = sliderY.Value;
        }

        // ?????????????????????? events ??????????????????????

        private void BtnSaveRestart_Click(object sender, EventArgs e)
        {
            WriteToSave();
            _onSaveAndRestart?.Invoke();
        }
    }
}
