using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace mavc_target_ui_win
{
    /**
     * Main application form.  Hosts a variable number of volume-group panels,
     * a menu bar, system-tray integration, and manages the background agent process.
     */
    public partial class Form1 : Form
    {
        #region Private Fields
        private string CURRENT_VERSION = "1.4.0";

        private AudioController audioController;

        private AODiscovery aodiscovery;

        public static string configSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MAVC");
        public static string configFileName = "config.json";
        public static string configFilePath = Path.Combine(configSavePath, configFileName);
        public static string selectedFilePath = configSavePath;

        private List<AudioOutput> availableOutputs;
        private static MAVCSave mavcSave;

        // general purpose timer for updating etc.
        Timer updateTimer = new Timer();

        // for notifying if there is a ui update
        ThreadSafeBool updateUIFlag = new ThreadSafeBool();

        Log logger = new Log(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MAVC", "ui-log.txt"));

        // System tray components
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        // Agent process management
        private Process agentProcess;
        private string agentExecutablePath;

        // Dynamic volume panel controls
        private List<GroupBox> volumeGroupBoxes = new List<GroupBox>();
        private List<VolumeListBox> volumeListBoxes = new List<VolumeListBox>();
        private List<ComboBox> addVolumeComboBoxes = new List<ComboBox>();
        private List<CheckBox> reverseCheckboxes = new List<CheckBox>();

        // Suppresses save() calls during UI population from config
        private bool isLoadingConfig = false;
        #endregion

        #region Public Methods
        /**
         * Applies the immersive dark-mode title bar attribute to this form.
         *
         * @param isDark  true to enable dark title bar, false for light
         */
        private void SetTitleBarTheme(bool isDark)
        {
            ThemeColors.SetTitleBarTheme(this.Handle, isDark);
        }

        /**
         * Returns the shared MAVCSave configuration instance.
         *
         * @return the current save-state object
         */
        public static MAVCSave GetMavcSave()
        {
            return mavcSave;
        }
        #endregion

        #region Constructor
        /**
         * Called after the form handle is created.  Hides the window
         * immediately when the "start minimized" setting is enabled.
         */
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (mavcSave != null && mavcSave.startMinimized)
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;

                // Immediately hide the form
                this.BeginInvoke(new System.Action(() =>
                {
                    this.Hide();
                }));
            }
        }

        /**
         * Constructs the main form: initializes components, tray icon, config,
         * and starts the agent process.
         */
        public Form1()
        {
            try
            {
                //load list of apps and devices
                InitializeComponent();

                // Initialize system tray icon
                InitializeTrayIcon();

                // Auto Check for update
                checkForUpdate();

                string uiLocation = AppDomain.CurrentDomain.BaseDirectory;
                logger.Info("UI running from: " + uiLocation);

                this.Text = "MAVC";
                this.versionLabel.Text = CURRENT_VERSION;

                try
                {
                    this.Icon = new System.Drawing.Icon("./icon.ico");
                }
                catch
                {
                    logger.Warning("App-Icon not found!");
                }

                mavcSave = new MAVCSave();
                mavcSave.EnsureCapacity();
                audioController = new AudioController();
                aodiscovery = new AODiscovery(audioController);

                loadConfig(configSavePath, configFileName);

                var devices = audioController.GetAudioDevices();
                foreach (var dev in devices)
                {
                    dev.OnOutputCreated((sender, newSession) =>
                    {
                        logger.Info("new output registered");
                        updateUIFlag.Value = true;
                    });
                }

                foreach (var ou in availableOutputs)
                {
                    logger.Info(ou.ToString());
                }

                updateTimer.Interval = 3000;   // milliseconds
                updateTimer.Tick += updateTimer_Tick;  // set handler
                updateTimer.Start();

                // Start the agent process
                StartAgentProcess();
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }
        }
        #endregion

        #region Dynamic Volume Panel Generation
        /**
         * Builds the dynamic volume panels in the tableLayoutPanel based on
         * the numberOfKnobs setting in mavcSave.
         */
        private void BuildVolumePanels()
        {
            // Clear existing dynamic controls
            ClearVolumePanels();

            int knobCount = mavcSave.numberOfKnobs;
            if (knobCount < 1) knobCount = 1;

            tableLayoutPanel1.SuspendLayout();

            // Configure table layout columns
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.ColumnCount = knobCount;
            float colPercent = 100f / knobCount;
            for (int i = 0; i < knobCount; i++)
            {
                tableLayoutPanel1.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Percent, colPercent));
            }

            for (int i = 0; i < knobCount; i++)
            {
                int index = i; // capture for closures

                // GroupBox
                var groupBox = new GroupBox();
                groupBox.Text = "Volume " + (i + 1);
                groupBox.Dock = DockStyle.Fill;
                groupBox.TabStop = false;
                groupBox.Padding = new Padding(6, 4, 6, 6);

                // Reverse Knob checkbox
                var reverseCheckbox = new CheckBox();
                reverseCheckbox.AutoSize = true;
                reverseCheckbox.Dock = DockStyle.Top;
                reverseCheckbox.Text = "Reverse Knob";
                reverseCheckbox.UseVisualStyleBackColor = true;
                reverseCheckbox.Padding = new Padding(0, 4, 0, 2);
                reverseCheckbox.CheckedChanged += (sender, e) =>
                {
                    if (isLoadingConfig) return;
                    if (index < mavcSave.reverseKnobs.Count)
                    {
                        mavcSave.reverseKnobs[index] = reverseCheckbox.Checked;
                        save(configSavePath, configFileName);
                    }
                };

                // Add Volume combo box
                var addVolCombo = new ComboBox();
                addVolCombo.Dock = DockStyle.Top;
                addVolCombo.DropDownStyle = ComboBoxStyle.DropDownList;
                addVolCombo.FormattingEnabled = true;
                addVolCombo.SelectedIndexChanged += (sender, e) =>
                {
                    AudioOutput selectedAO = (AudioOutput)addVolCombo.SelectedItem;
                    if (selectedAO != null)
                    {
                        volumeListBoxes[index].Items.Add(selectedAO);
                        removeAvailableOutput(selectedAO);
                        addVolCombo.SelectedIndex = -1;
                    }
                };

                // Spacer between combo and list
                var spacer = new Panel();
                spacer.Dock = DockStyle.Top;
                spacer.Height = 4;

                // Volume list box
                var volList = new VolumeListBox();
                volList.Dock = DockStyle.Fill;
                volList.FormattingEnabled = true;
                volList.IntegralHeight = false;
                volList.SelectionMode = SelectionMode.MultiSimple;

                // Add controls bottom-up so Dock stacking works correctly:
                // Fill (volList) first, then top items stack from top
                groupBox.Controls.Add(volList);
                groupBox.Controls.Add(spacer);
                groupBox.Controls.Add(addVolCombo);
                groupBox.Controls.Add(reverseCheckbox);

                tableLayoutPanel1.Controls.Add(groupBox, i, 0);

                volumeGroupBoxes.Add(groupBox);
                volumeListBoxes.Add(volList);
                addVolumeComboBoxes.Add(addVolCombo);
                reverseCheckboxes.Add(reverseCheckbox);
            }

            tableLayoutPanel1.ResumeLayout(true);

            // Apply current theme to newly created controls
            if (mavcSave != null)
            {
                ThemeColors.UpdateControlTheme(tableLayoutPanel1, mavcSave.darkMode);
            }
        }

        /** Removes all dynamically created volume panels. */
        private void ClearVolumePanels()
        {
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.ResumeLayout(false);

            foreach (var gb in volumeGroupBoxes)
                gb.Dispose();

            volumeGroupBoxes.Clear();
            volumeListBoxes.Clear();
            addVolumeComboBoxes.Clear();
            reverseCheckboxes.Clear();
        }
        #endregion

        #region Agent Process Management
        /** Starts the agent process. */
        private void StartAgentProcess()
        {
            try
            {
                // stop tracked instance first (failed attempts / stale handle)
                try
                {
                    if (agentProcess != null)
                    {
                        if (!agentProcess.HasExited)
                        {
                            agentProcess.Kill();
                            agentProcess.WaitForExit(2000);
                        }
                    }
                }
                catch { }
                finally
                {
                    try { agentProcess?.Dispose(); } catch { }
                    agentProcess = null;
                }

                // kill any other agent processes
                KillExistingAgentProcesses();

                // find agent executable
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                // Detect if we are running from a build output directory (IDE scenario).
                // Build output paths look like: ...\bin\Debug\  or  ...\bin\Release\
                bool isDevelopment = false;
                string devAgentPath = null;

                string normalizedBase = baseDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string parentName = Path.GetFileName(Path.GetDirectoryName(normalizedBase)) ?? "";

                if (parentName.Equals("bin", StringComparison.OrdinalIgnoreCase))
                {
                    // baseDir is  ...\bin\<Config>\  (running from build output)
                    string buildConfig = Path.GetFileName(normalizedBase); // "Debug" or "Release"
                    isDevelopment = true;
                    devAgentPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..",
                        "mavc-target-agent", "mavc-target-agent", "bin", buildConfig, "net8.0-windows", "mavc-target-agent.exe"));
                }

                string[] possiblePaths;

                if (isDevelopment)
                {
                    // Running from IDE (prefer the matching build-config agent)
                    possiblePaths = new string[]
                    {
                        devAgentPath
                    };
                }
                else
                {
                    // Installed / portable (production paths only)
                    possiblePaths = new string[]
                    {
                        // Same directory as UI (portable deployment)
                        Path.Combine(baseDir, "agent", "mavc-target-agent.exe"),

                        // Production location installed via MSI
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                            "Mavc", "Mavc", "agent", "mavc-target-agent.exe"),

                        // Alternative production location (64-bit Program Files)
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                            "Mavc", "Mavc", "agent", "mavc-target-agent.exe"),
                    };
                }

                agentExecutablePath = null;

                foreach (string path in possiblePaths)
                {
                    try
                    {
                        string fullPath = Path.GetFullPath(path);
                        logger.Info($"Checking for agent at: {fullPath}");

                        if (File.Exists(fullPath))
                        {
                            agentExecutablePath = fullPath;
                            logger.Info($"Found agent executable at: {agentExecutablePath}");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warning($"Error checking path {path}: {ex.Message}");
                    }
                }

                if (agentExecutablePath == null)
                {
                    string errorMessage =
                        "Agent executable not found.\n\n" +
                        "Please ensure MAVC is installed or build the mavc-target-agent project.";
                    logger.Warning("Agent executable not found in any location.");
                    MessageBox.Show(errorMessage, "Agent Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // start agent
                agentProcess = new Process();
                agentProcess.StartInfo.FileName = agentExecutablePath;
                agentProcess.StartInfo.UseShellExecute = false;

                // allow console window when debug mode is enabled
                agentProcess.StartInfo.CreateNoWindow = !mavcSave.enableDebugMode;
                agentProcess.StartInfo.WindowStyle = mavcSave.enableDebugMode
                    ? ProcessWindowStyle.Normal
                    : ProcessWindowStyle.Hidden;

                agentProcess.EnableRaisingEvents = true;
                agentProcess.Exited += (sender, e) =>
                {
                    logger.Warning("Agent process exited unexpectedly.");
                };

                agentProcess.Start();
                logger.Info($"Agent process started successfully from: {agentExecutablePath}");

                // validate and cleanup "failed attempts"
                System.Threading.Thread.Sleep(250);
                if (agentProcess.HasExited)
                {
                    int code = agentProcess.ExitCode;
                    try { agentProcess.Dispose(); } catch { }
                    agentProcess = null;
                    throw new Exception($"Agent exited immediately with code {code}.");
                }

                trayIcon.ShowBalloonTip(2000, "MAVC", "Agent started successfully", ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to start agent process: " + ex);

                string errorMessage =
                    "Failed to start agent process.\n\n" +
                    ex.Message + "\n\n" +
                    "The UI will continue to run, but the agent will need to be started manually.";

                MessageBox.Show(errorMessage, "Agent Start Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /** Kills all existing agent processes that are currently running. */
        private void KillExistingAgentProcesses()
        {
            try
            {
                Process[] existingProcesses = Process.GetProcessesByName("mavc-target-agent");

                if (existingProcesses.Length > 0)
                {
                    logger.Info($"Found {existingProcesses.Length} existing agent process(es). Terminating...");

                    foreach (Process proc in existingProcesses)
                    {
                        try
                        {
                            // Check if process has already exited before trying to access its properties
                            if (proc.HasExited)
                            {
                                logger.Info($"Process already exited, skipping.");
                                proc.Dispose();
                                continue;
                            }

                            int processId = proc.Id; // Store PID before killing
                            logger.Info($"Killing agent process with PID: {processId}");

                            proc.Kill();
                            proc.WaitForExit(2000); // Wait up to 2 seconds for each process
                            proc.Dispose();

                            logger.Info($"Successfully terminated agent process with PID: {processId}");
                        }
                        catch (InvalidOperationException)
                        {
                            // Process has already exited or is no longer accessible
                            logger.Info($"Process already exited or inaccessible, skipping.");
                            try { proc.Dispose(); } catch { }
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Failed to kill process: {ex.Message}");
                            try { proc.Dispose(); } catch { }
                        }
                    }

                    // wait for all processes to fully terminate
                    System.Threading.Thread.Sleep(500);
                    logger.Info("All existing agent processes terminated.");
                }
                else
                {
                    logger.Info("No existing agent processes found.");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error checking for existing agent processes: {ex.Message}");
            }
        }

        /** Stops the agent process. */
        private void StopAgentProcess()
        {
            try
            {
                if (agentProcess != null && !agentProcess.HasExited)
                {
                    logger.Info("Stopping agent process...");
                    agentProcess.Kill();
                    agentProcess.WaitForExit(5000); // Wait up to 5 seconds
                    agentProcess.Dispose();
                    agentProcess = null;
                    logger.Info("Agent process stopped successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error stopping agent process: {ex}");
            }
        }

        /** Restarts the agent process. */
        private void RestartAgentProcess()
        {
            logger.Info("Restarting agent process...");

            // Stop our tracked agent process
            StopAgentProcess();

            // Also kill any other agent processes that might be running
            KillExistingAgentProcesses();

            System.Threading.Thread.Sleep(500); // Give it a moment to fully terminate
            StartAgentProcess();
        }
        #endregion

        #region Tray Icon Management
        /** Initializes the system tray icon and context menu. */
        private void InitializeTrayIcon()
        {
            // Create the tray icon
            trayIcon = new NotifyIcon();

            try
            {
                trayIcon.Icon = new System.Drawing.Icon("./icon.ico");
            }
            catch
            {
                trayIcon.Icon = this.Icon;
                logger.Warning("Tray icon not found, using default!");
            }

            trayIcon.Text = "MAVC - Multi-App Volume Control";
            trayIcon.Visible = true;

            // Create the context menu
            trayMenu = new ContextMenuStrip();

            ToolStripMenuItem openUIItem = new ToolStripMenuItem("Open UI", null, OnOpenUI);
            ToolStripMenuItem restartAgentItem = new ToolStripMenuItem("Restart Agent", null, OnRestartAgent);
            ToolStripSeparator separator = new ToolStripSeparator();
            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit", null, OnExit);

            trayMenu.Items.Add(openUIItem);
            trayMenu.Items.Add(restartAgentItem);
            trayMenu.Items.Add(separator);
            trayMenu.Items.Add(exitItem);

            // Attach menu to tray icon
            trayIcon.ContextMenuStrip = trayMenu;

            // Double-click to show UI
            trayIcon.DoubleClick += (s, e) => ShowUI();
        }

        /** Event handler for "Open UI" menu item. */
        private void OnOpenUI(object sender, EventArgs e)
        {
            ShowUI();
        }

        /** Event handler for "Restart Agent" menu item. */
        private void OnRestartAgent(object sender, EventArgs e)
        {
            try
            {
                RestartAgentProcess();

                // Show notification
                trayIcon.ShowBalloonTip(2000, "MAVC", "Agent restarted successfully", ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to restart agent: " + ex.ToString());
                MessageBox.Show("Failed to restart agent. Check logs for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /** Event handler for "Exit" menu item. */
        private void OnExit(object sender, EventArgs e)
        {
            // Confirm exit
            DialogResult result = MessageBox.Show(
                "Are you sure you want to exit MAVC? The agent will stop running.",
                "Exit MAVC",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                logger.Info("Exiting application");

                // Stop the agent process
                StopAgentProcess();

                trayIcon.Visible = false;
                updateTimer.Stop();
                System.Windows.Forms.Application.Exit();
            }
        }

        /** Shows the UI window. */
        private void ShowUI()
        {
            this.ShowInTaskbar = true; // before showing the window to prevent default icon
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate();

            // Defer focus reset so WinForms' own focus pass doesn't override it
            this.BeginInvoke(new Action(() =>
            {
                if (this.ActiveControl is Button)
                    this.ActiveControl = null;
            }));
            if (mavcSave != null)
                SetTitleBarTheme(mavcSave.darkMode);
        }

        #endregion

        #region Form Event Handlers
        /** Override form closing to minimize to tray instead of closing if toggle is enabled. */
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && minimizeOnCloseToolStripMenuItem.Checked)
            {
                // Hide to tray instead of closing
                e.Cancel = true;
                this.Hide();
                this.ShowInTaskbar = false;

                // Show notification first time
                if (trayIcon.Tag == null)
                {
                    trayIcon.ShowBalloonTip(2000, "MAVC", "Application minimized to tray. Agent is still running.", ToolTipIcon.Info);
                    trayIcon.Tag = "shown";
                }
            }
            else if (e.CloseReason == CloseReason.UserClosing)
            {
                // User is closing without minimize to tray - stop the agent
                StopAgentProcess();
            }
            base.OnFormClosing(e);
        }
        #endregion

        #region Update Methods
        /**
         * Checks the latest GitHub release tag against CURRENT_VERSION
         * and prompts the user to update when a newer version exists.
         */
        private void checkForUpdate()
        {
            Version latestGitHubVersion = null;
            Version localVersion = null;
            try
            {
                logger.Info("Checking for latest version available...");

                GitHubClient client = new GitHubClient(new ProductHeaderValue("MavcAutoUpdater"));
                IReadOnlyList<Release> releases = Task.Run(() => client.Repository.Release.GetAll("DavidGitter", "multi-app-volume-control")).GetAwaiter().GetResult();

                logger.Info("Latest Release Tag found: " + releases[0].TagName);

                //Setup the versions
                latestGitHubVersion = new Version(releases[0].TagName);
                localVersion = new Version(CURRENT_VERSION); //Replace this with your local version. 
            }
            catch (Exception e)
            {
                logger.Error("An error occured while checking for available updates: " + e.ToString());
            }                                                                     //Only tested with numeric values.

            try
            {
                //Compare the Versions
                //Source: https://stackoverflow.com/questions/7568147/compare-version-numbers-without-using-split-function
                int versionComparison = localVersion.CompareTo(latestGitHubVersion);
                if (versionComparison < 0 && localVersion != null && latestGitHubVersion != null)
                {
                    //The version on GitHub is more up to date than this local release.
                    updateApplication();
                }
                else if (versionComparison > 0)
                {
                    //This local version is greater than the release version on GitHub.
                }
                else
                {
                    //This local Version and the Version on GitHub are equal.
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occured while trying to install the latest update: " + e.ToString());
            }
        }

        /**
         * Downloads the latest MSI installer from GitHub, extracts it, and
         * launches the installer before closing the current instance.
         */
        private void updateApplication()
        {
            WebClient client = new WebClient();

            string repoLink = "https://github.com/DavidGitter/multi-app-volume-control";
            string repoLatestRelease = repoLink + "/releases/latest/download/MavcSetup.zip";

            if (MessageBox.Show("A new update is available! Do you want to download it?", "Update Available!", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    string mavcFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MAVC");
                    string msiFilePath = Path.Combine(mavcFolderPath, "MavcSetup.msi");
                    string zipFilePath = Path.Combine(mavcFolderPath, "MavcSetup.zip");
                    if (System.IO.File.Exists(msiFilePath)) { System.IO.File.Delete(msiFilePath); }
                    client.DownloadFile(repoLatestRelease, zipFilePath);
                    string zipPath = zipFilePath;
                    string extractPath = mavcFolderPath;
                    ZipFile.ExtractToDirectory(zipPath, extractPath);
                    Process process = new Process();
                    process.StartInfo.FileName = "msiexec.exe";
                    process.StartInfo.Arguments = string.Format("/i " + msiFilePath);
                    this.Close();
                    process.Start();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                }
            }
        }

        /**
         * Timer tick handler.  Polls updateUIFlag and refreshes the available
         * audio outputs when a new session is detected.
         */
        private void updateTimer_Tick(object sender, EventArgs e)
        {

            //check for new audio outputs
            if (updateUIFlag.Value == true)
            {
                updateUIFlag.Value = false;
                refreshAvailableOutputs();
                // Don't call loadFromMavcSave here — it rebuilds panels unnecessarily
                // and wipes the combo box contents. Just refresh the available outputs.
            }
        }
        #endregion

        #region Available Outputs Management
        /**
         * Populates all "Add Volume" combo boxes with outputs that are not
         * already assigned to a volume list, plus the special Focused and
         * Other Apps function entries.
         *
         * @param availableOutputs  all currently available audio outputs
         */
        private void initAvailableOutputs(AudioOutput[] availableOutputs)
        {
            foreach (var output in availableOutputs)
            {
                if (!confHasAudioOutput(output))
                {
                    foreach (var combo in addVolumeComboBoxes)
                        combo.Items.Add(output);
                }
            }

            //add additional functions
            AudioFocused af = new AudioFocused(audioController);
            AudioOtherApps aoa = new AudioOtherApps(audioController, mavcSave);

            foreach (var combo in addVolumeComboBoxes)
            {
                combo.Items.Add(af);
                combo.Items.Add(aoa);
            }
        }

        /** Refreshes the available outputs. */
        private void refreshAvailableOutputs()
        {
            availableOutputs.Clear();
            removeAvailableOutputs();
            availableOutputs = aodiscovery.GetAllAudioOutputs();

            logger.Info($"Refresh complete: found {availableOutputs.Count} available audio outputs");

            if (availableOutputs.Count == 0)
            {
                logger.Warning("No new audio outputs found during refresh");
            }
            else
            {
                foreach (var output in availableOutputs)
                {
                    logger.Info($"  - {output.GetName()} ({output.GetAudioType()})");
                }
            }

            initAvailableOutputs(availableOutputs.ToArray());
        }

        /**
         * Removes a single audio output from all combo boxes.
         *
         * @param output  the audio output to remove
         */
        private void removeAvailableOutput(AudioOutput output)
        {
            foreach (var combo in addVolumeComboBoxes)
                combo.Items.Remove(output);
        }

        /** Clears every item from all combo boxes. */
        private void removeAvailableOutputs()
        {
            foreach (var combo in addVolumeComboBoxes)
                combo.Items.Clear();
        }

        /**
         * Adds a single audio output to all combo boxes.
         *
         * @param output  the audio output to add
         */
        private void addAvailableOutput(AudioOutput output)
        {
            foreach (var combo in addVolumeComboBoxes)
                combo.Items.Add(output);
        }

        /**
         * Checks whether any of the volume lists in the current config
         * already contain the specified audio output (by name).
         *
         * @param ao  the audio output to look for
         * @return true if the output is already assigned to a volume list
         */
        private bool confHasAudioOutput(AudioOutput ao)
        {
            foreach (var mapping in mavcSave.volumeMappings)
            {
                if (mapping.Exists(mavc_ao => ao.GetName().Equals(mavc_ao.name)))
                    return true;
            }
            return false;
        }
        #endregion

        #region Configuration Management
        /**
         * "Save" button click handler.  Persists the current config to disk
         * and restarts the agent so it picks up the changes.
         */
        private void saveBtn_Click(object sender, EventArgs e)
        {
            save(configSavePath, configFileName);
            RestartAgentProcess();
        }

        /** Copies the current volume-list contents back into mavcSave. */
        private void updateMavcSave()
        {
            // Don't overwrite config when the UI panels haven't been populated yet
            if (volumeListBoxes.Count == 0)
                return;

            for (int i = 0; i < mavcSave.numberOfKnobs && i < mavcSave.volumeMappings.Count; i++)
            {
                mavcSave.volumeMappings[i].Clear();
            }

            for (int i = 0; i < volumeListBoxes.Count && i < mavcSave.numberOfKnobs; i++)
            {
                foreach (AudioOutput ao in volumeListBoxes[i].Items)
                    mavcSave.volumeMappings[i].Add(new MAVCSave.AudioOutput(ao.GetName(), ao.GetAudioType()));
            }

            // Trim excess mappings/reverseKnobs that exceed numberOfKnobs
            while (mavcSave.volumeMappings.Count > mavcSave.numberOfKnobs)
                mavcSave.volumeMappings.RemoveAt(mavcSave.volumeMappings.Count - 1);
            while (mavcSave.reverseKnobs.Count > mavcSave.numberOfKnobs)
                mavcSave.reverseKnobs.RemoveAt(mavcSave.reverseKnobs.Count - 1);

            // Persist current window size and position
            if (this.WindowState == FormWindowState.Normal)
            {
                mavcSave.windowWidth = this.Size.Width;
                mavcSave.windowHeight = this.Size.Height;
                mavcSave.windowX = this.Location.X;
                mavcSave.windowY = this.Location.Y;
            }
        }

        /**
         * Loads all settings and volume-list entries from mavcSave
         * into the UI controls (list boxes, checkboxes, menu items, etc.).
         */
        private void loadFromMavcSave()
        {
            isLoadingConfig = true;

            // Rebuild volume panels to match current numberOfKnobs
            BuildVolumePanels();

            ClearVolLists();

            int knobCount = mavcSave.numberOfKnobs;

            // Load volume mappings into list boxes
            try
            {
                var tasks = new Task[knobCount];
                var foundOutputsPerKnob = new List<AudioOutput>[knobCount];

                for (int i = 0; i < knobCount; i++)
                {
                    int index = i; // capture
                    foundOutputsPerKnob[index] = new List<AudioOutput>();

                    tasks[index] = Task.Run(() =>
                    {
                        if (index >= mavcSave.volumeMappings.Count) return;

                        foreach (MAVCSave.AudioOutput mavc_ao in mavcSave.volumeMappings[index])
                        {
                            try
                            {
                                if (!mavc_ao.type.Equals("Function"))
                                    foundOutputsPerKnob[index].Add(audioController.GetOutputByName(mavc_ao.name));
                                else if (mavc_ao.name.Equals("Focused"))
                                    foundOutputsPerKnob[index].Add(new AudioFocused(audioController));
                                else if (mavc_ao.name.Equals("Other Apps"))
                                    foundOutputsPerKnob[index].Add(new AudioOtherApps(audioController, mavcSave));
                                else
                                    throw new NotImplementedException();
                            }
                            catch (Exception)
                            {
                                logger.Warning("AudioOutput " + mavc_ao.name + " of mavc save not found");
                                foundOutputsPerKnob[index].Add(new AudioOutputOffline(mavc_ao.name));
                            }
                        }
                    });
                }

                Task.WaitAll(tasks);

                for (int i = 0; i < knobCount && i < volumeListBoxes.Count; i++)
                {
                    volumeListBoxes[i].Items.AddRange(foundOutputsPerKnob[i].ToArray());
                }
            }
            catch (Exception e)
            {
                logger.Error("Error loading volume mappings: " + e.Message + "\n" + e.StackTrace);
            }

            // Load UI settings (always apply, independent of volume loading)
            try
            {
                // update knob-reversed checkboxes
                for (int i = 0; i < knobCount && i < reverseCheckboxes.Count; i++)
                {
                    reverseCheckboxes[i].Checked = mavcSave.reverseKnobs[i];
                }

                // update knob order (now in menu strip)
                reverseKnobOrderToolStripMenuItem.Checked = mavcSave.reverseKnobOrder;

                // load darkmode state
                darkModeToolStripMenuItem.Checked = mavcSave.darkMode;
                ApplyTheme(mavcSave.darkMode);

                // load minimize on close setting (now in menu strip)
                minimizeOnCloseToolStripMenuItem.Checked = mavcSave.minimizeOnClose;

                // update enable debug mode (now in menu strip)
                enableDebugModeToolStripMenuItem.Checked = mavcSave.enableDebugMode;

                // load start minimized setting (now in menu strip)
                startMinimizedToolStripMenuItem.Checked = mavcSave.startMinimized;

                // restore window size and position
                if (mavcSave.windowWidth > 0 && mavcSave.windowHeight > 0)
                {
                    this.Size = new Size(mavcSave.windowWidth, mavcSave.windowHeight);
                }
                if (mavcSave.windowX != int.MinValue && mavcSave.windowY != int.MinValue)
                {
                    this.StartPosition = FormStartPosition.Manual;
                    this.Location = new Point(mavcSave.windowX, mavcSave.windowY);
                }
            }
            catch (Exception e)
            {
                logger.Error("Error loading UI settings: " + e.Message + "\n" + e.StackTrace);
            }

            isLoadingConfig = false;
        }

        /**
         * Saves the config saves to a specified file
         *
         * @param path  folder path for the config file
         * @param file  file name (e.g. config.json)
         */
        private void save(string path, string file)
        {
            updateMavcSave();
            string fullPath = Path.Combine(path, file);
            if (!System.IO.File.Exists(fullPath))
            {
                Directory.CreateDirectory(path);
            }

            // Serialize the class to JSON
            string json = JsonConvert.SerializeObject(mavcSave, Formatting.Indented);

            // Save the JSON string to a file
            System.IO.File.WriteAllText(fullPath, json);
        }

        /**
         * Menu-bar "Save" handler.  Falls back to saveTo when no
         * save path has been set yet.
         */
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (configSavePath == null)
            {
                saveTo();
            }
            else
            {
                save(configSavePath, configFileName);
                RestartAgentProcess();
            }
        }

        /** Opens a SaveFileDialog and saves the config to the user-chosen location. */
        private void saveTo()
        {
            string selectedFilePath = null;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "MAVC Config File|*.mavc";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFilePath = saveFileDialog.FileName;
                logger.Info("Selected file: " + selectedFilePath);
            }

            save(selectedFilePath, configFileName);
        }

        /** Menu-bar "Save To…" handler.  Delegates to saveTo. */
        private void SaveToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveTo();
        }

        /**
         * Deserialises a config file from disk, populates mavcSave,
         * loads the UI, and initialises the available audio outputs.
         *
         * @param configFileFolder  folder that contains the config file
         * @param configFileName    name of the config file
         */
        private void loadConfig(string configFileFolder, string configFileName)
        {
            try
            {
                string configFilePath = Path.Combine(configFileFolder, configFileName);
                mavcSave = MAVCSave.LoadConfigFromFile(configFilePath, configSavePath);
            }
            catch
            {
                logger.Warning("Config file " + configFilePath + " propably not existing, creating new one...");
                save(configSavePath, configFileName);

            }

            loadFromMavcSave();
            availableOutputs = aodiscovery.GetAllAudioOutputs();
            initAvailableOutputs(availableOutputs.ToArray());
        }

        /**
         * Menu-bar "Open" handler.  Prompts the user to discard unsaved changes,
         * then loads a config file chosen via OpenFileDialog.
         */
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO: unsafed changes discard question here!
            DialogResult result = MessageBox.Show("Do you want to discard your changes?", "Discard Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                // continoue opening
                string selectedFilePath = null;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Open Config File";
                openFileDialog.Filter = "MAVC Config File|*.json";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = openFileDialog.FileName;
                    logger.Info("Selected file: " + selectedFilePath);
                }

                //TODO: check if config is valid otherwise abort load

                ClearVolLists();
                loadConfig(Path.GetDirectoryName(selectedFilePath), Path.GetFileName(selectedFilePath));
            }
            else
            {
                // stop opening
                logger.Info("User clicked No.");
            }
        }
        #endregion

        #region Volume List Event Handlers
        /**
         * "Delete Selection" button handler.  Removes every selected audio output
         * from all volume lists and adds them back to the combo boxes.
         */
        private void delItemBtn_Click(object sender, EventArgs e)
        {
            List<AudioOutput> selectedItems = new List<AudioOutput>();

            foreach (var volList in volumeListBoxes)
            {
                foreach (AudioOutput ao in volList.SelectedItems)
                    selectedItems.Add(ao);
            }

            foreach (AudioOutput ao in selectedItems)
            {
                foreach (var volList in volumeListBoxes)
                    volList.Items.Remove(ao);
                addAvailableOutput(ao);
            }
        }

        /**
         * "Discard Selection" button handler.  Clears the selection highlight
         * in all volume list boxes without removing items.
         */
        private void discSelBtn_Click(object sender, EventArgs e)
        {
            foreach (var volList in volumeListBoxes)
                volList.ClearSelected();
        }

        /** Removes all items from all volume list boxes. */
        private void ClearVolLists()
        {
            foreach (var volList in volumeListBoxes)
                volList.Items.Clear();
        }
        #endregion

        #region UI Control Event Handlers
        /**
         * Menu-bar "Refresh" handler.  Re-scans audio outputs and updates
         * the available outputs in combo boxes without rebuilding the UI.
         */
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logger.Info("User triggered refresh of available audio outputs");
            // Only refresh available outputs — don't rebuild panels
            refreshAvailableOutputs();
        }

        /**
         * Menu-bar "Dark Mode" toggle handler.  Flips the dark-mode flag,
         * applies the theme, and persists the setting.
         */
        private void darkModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mavcSave.darkMode = !mavcSave.darkMode;
            save(configSavePath, configFileName);
            // Rebuild panels and reapply theme from scratch so combo boxes
            // get recreated with the correct native styling.
            loadFromMavcSave();
            refreshAvailableOutputs();
        }

        /**
         * Applies the dark or light theme to this form and updates the
         * menu-item check state.
         *
         * @param isDark  true for dark mode, false for light mode
         */
        private void ApplyTheme(bool isDark)
        {
            ThemeColors.ApplyTheme(this, isDark);
            darkModeToolStripMenuItem.Checked = isDark;
        }

        /** "Reverse Knob Order" settings toggle. Persists immediately. */
        private void reverseKnobOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mavcSave.reverseKnobOrder = !mavcSave.reverseKnobOrder;
            reverseKnobOrderToolStripMenuItem.Checked = mavcSave.reverseKnobOrder;
            save(configSavePath, configFileName);
        }

        /** "Enable Debug Mode" settings toggle. Persists and restarts agent immediately. */
        private void enableDebugModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mavcSave.enableDebugMode = !mavcSave.enableDebugMode;
            enableDebugModeToolStripMenuItem.Checked = mavcSave.enableDebugMode;
            save(configSavePath, configFileName);
            RestartAgentProcess();
        }

        /** "Minimize on Close" settings toggle. Persists immediately. */
        private void minimizeOnCloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mavcSave.minimizeOnClose = !mavcSave.minimizeOnClose;
            minimizeOnCloseToolStripMenuItem.Checked = mavcSave.minimizeOnClose;
            save(configSavePath, configFileName);
        }

        /** "Start Minimized to Systemtray" settings toggle. Persists immediately. */
        private void startMinimizedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mavcSave.startMinimized = !mavcSave.startMinimized;
            startMinimizedToolStripMenuItem.Checked = mavcSave.startMinimized;
            save(configSavePath, configFileName);
        }

        /** "Number of Knobs..." settings handler. Prompts for the number and rebuilds the UI. */
        private void numberOfKnobsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new NumberOfKnobsForm(mavcSave.numberOfKnobs, mavcSave.darkMode))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedValue != mavcSave.numberOfKnobs)
                {
                    mavcSave.SetNumberOfKnobs(dlg.SelectedValue);
                    save(configSavePath, configFileName);
                    loadFromMavcSave();
                    refreshAvailableOutputs();
                }
            }
        }

        /** Opens the Overlay Settings dialog. */
        private void overlaySettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new OverlaySettingsForm(mavcSave, mavcSave.darkMode, () =>
            {
                save(configSavePath, configFileName);
                RestartAgentProcess();
            },
            () =>
            {
                save(configSavePath, configFileName);
            });
            dlg.ShowDialog(this);
        }
        #endregion

        #region Miscellaneous Event Handlers
        /** Menu-bar "Help" handler. Not yet implemented. */
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

/**
 * ListBox subclass used for the volume mapping lists.
 *
 * Overrides the Windows message handler to ignore left-clicks that land on
 * empty space below the last item.  Without this override, WinForms'
 * MultiSimple selection mode would accidentally toggle the first item
 * whenever the user clicks anywhere in the empty area of the list.
 *
 * @see ListBox
 */
internal class VolumeListBox : ListBox
{
    protected override void WndProc(ref Message m)
    {
        const int LeftButtonDown = 0x0201;
        const int LeftButtonDoubleClick = 0x0203;

        if (m.Msg == LeftButtonDown || m.Msg == LeftButtonDoubleClick)
        {
            // LParam encodes the cursor position as two packed 16-bit values.
            int x = m.LParam.ToInt32() & 0xFFFF;
            int y = (m.LParam.ToInt32() >> 16) & 0xFFFF;

            // If the click didn't land on any item, swallow the message.
            if (IndexFromPoint(x, y) == NoMatches)
                return;
        }

        base.WndProc(ref m);
    }
}
