using Newtonsoft.Json;
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

namespace mavc_target_ui_win
{
    public partial class Form1 : Form
    {
        #region Private Fields
        private string CURRENT_VERSION = "1.3.1";

        private AudioController audioController;
        public static string configSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MAVC");
        public static string configFileName = "config.json";
        public static string configFilePath = Path.Combine(configSavePath, configFileName);
        public static string selectedFilePath = configSavePath;

        private List<AudioOutput> availableOutputs;
        private static MAVCSave mavcSave;

        // general purpose timer for updating etc.
        Timer updateTimer = new Timer();

        // timer for saving overlay position changes
        private readonly Timer overlayLiveTimer = new Timer();
        private bool overlayPosDirty = false;

        // for notifying if there is a ui update
        ThreadSafeBool updateUIFlag = new ThreadSafeBool();

        Log logger = new Log(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MAVC", "ui-log.txt"));

        // System tray components
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        // Agent process management
        private Process agentProcess;
        private string agentExecutablePath;
        #endregion

        #region Public Methods
        private void SetTitleBarTheme(bool isDark)
        {
            ThemeColors.SetTitleBarTheme(this.Handle, isDark);
        }

        public static MAVCSave GetMavcSave()
        {
            return mavcSave;
        }
        #endregion

        #region Constructor
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


                this.Text = "MAVC";
                this.versionText.Text = CURRENT_VERSION;

                try
                {
                    this.Icon = new System.Drawing.Icon("./icon.ico");
                }
                catch
                {
                    logger.Warning("App-Icon not found!");
                }

                mavcSave = new MAVCSave();
                audioController = new AudioController();

                loadConfig(configSavePath, configFileName);

                var devices = audioController.GetAudioDevices();
                foreach (var dev in devices)
                {
                    dev.OnOutputCreated((sender, newSession) =>
                    {
                        Console.WriteLine("new output registered");
                        updateUIFlag.Value = true;
                    });
                }

                foreach (var ou in availableOutputs)
                {
                    Console.WriteLine(ou.ToString());
                }

                updateTimer.Interval = 3000;   // milliseconds
                updateTimer.Tick += updateTimer_Tick;  // set handler
                updateTimer.Start();

                autoHideAfterSectoolStripTextBox.KeyDown += (s, e) =>
                {
                    try
                    {
                        if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
                        {
                            if (!autoHideAfterSectoolStripTextBox.Text.All(char.IsDigit))
                            {
                                MessageBox.Show("Input not a number.");
                                e.SuppressKeyPress = true;
                                return;
                            }

                            Debug.WriteLine("autohideafter: " + autoHideAfterSectoolStripTextBox.Text);
                            mavcSave.autoHideAfterSec =
                                int.Parse(autoHideAfterSectoolStripTextBox.Text);
                        }
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine(e);
                    }
                };

                // overlay position controls
                overlayLiveTimer.Interval = 30; // ~33fps; try 16 for ~60fps if your PC can handle it
                overlayLiveTimer.Tick += (s, e) =>
                {
                    if (!overlayPosDirty) return;
                    overlayPosDirty = false;

                    // write config so agent's FileSystemWatcher reloads and moves overlay
                    save(configSavePath, configFileName);
                };

                void markDirty()
                {
                    overlayPosDirty = true;
                    if (!overlayLiveTimer.Enabled) overlayLiveTimer.Start();
                }

                // While editing: update values + request a (throttled) save
                numericUpDown1.ValueChanged += (s, e) => { mavcSave.overlayX = (int)numericUpDown1.Value; markDirty(); };
                numericUpDown2.ValueChanged += (s, e) => { mavcSave.overlayY = (int)numericUpDown2.Value; markDirty(); };

                // Only do live updates while focused
                numericUpDown1.Enter += (s, e) => overlayLiveTimer.Start();
                numericUpDown2.Enter += (s, e) => overlayLiveTimer.Start();

                numericUpDown1.Leave += (s, e) => { overlayLiveTimer.Stop(); overlayPosDirty = false; };
                numericUpDown2.Leave += (s, e) => { overlayLiveTimer.Stop(); overlayPosDirty = false; };


                // Start the agent process
                StartAgentProcess();
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }
        }
        #endregion

        #region Agent Process Management
        /// <summary>
        /// Starts the agent process
        /// </summary>
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
                KillExistingAgentProcesses(); // you already have this method [file:18]

                // find agent executable
                string[] possiblePaths;

#if DEBUG
                possiblePaths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
                    "mavc-target-agent", "mavc-target-agent", "bin", "Debug", "net6.0-windows", "mavc-target-agent.exe"),
};
#else
                possiblePaths = new string[]
                {
                    // Production location installed via MSI - CHECK FIRST in production
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                        "Mavc", "Mavc", "agent", "mavc-target-agent.exe"),

                    // Alternative production location (64-bit Program Files)
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                        "Mavc", "Mavc", "agent", "mavc-target-agent.exe"),

                    // Same directory as UI (portable deployment)
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agent", "mavc-target-agent.exe"),

                    // Development location fallback
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
                        "mavc-target-agent", "mavc-target-agent", "bin", "Debug", "net6.0-windows", "mavc-target-agent.exe"),
                };
#endif

                agentExecutablePath = null;

                foreach (string path in possiblePaths)
                {
                    try
                    {
                        string fullPath = Path.GetFullPath(path);
                        Debug.WriteLine($"Checking for agent at: {fullPath}");

                        if (File.Exists(fullPath))
                        {
                            agentExecutablePath = fullPath;
                            Debug.WriteLine($"Found agent executable at: {agentExecutablePath}");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error checking path {path}: {ex.Message}");
                    }
                }

                if (agentExecutablePath == null)
                {
                    string errorMessage =
                        "Agent executable not found.\n\n" +
                        "Please ensure MAVC is installed or build the mavc-target-agent project.";
                    Debug.WriteLine("Agent executable not found in any location.");
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
                    Debug.WriteLine("Agent process exited unexpectedly.");
                };

                agentProcess.Start();
                Debug.WriteLine($"Agent process started successfully from: {agentExecutablePath}");

                // validate and cleanup “failed attempts”
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
                Debug.WriteLine("Failed to start agent process: " + ex);

                string errorMessage =
                    "Failed to start agent process.\n\n" +
                    ex.Message + "\n\n" +
                    "The UI will continue to run, but the agent will need to be started manually.";

                MessageBox.Show(errorMessage, "Agent Start Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Kills all existing agent processes that are currently running
        /// </summary>
        private void KillExistingAgentProcesses()
        {
            try
            {
                Process[] existingProcesses = Process.GetProcessesByName("mavc-target-agent");

                if (existingProcesses.Length > 0)
                {
                    Debug.WriteLine($"Found {existingProcesses.Length} existing agent process(es). Terminating...");

                    foreach (Process proc in existingProcesses)
                    {
                        try
                        {
                            // Check if process has already exited before trying to access its properties
                            if (proc.HasExited)
                            {
                                Debug.WriteLine($"Process already exited, skipping.");
                                proc.Dispose();
                                continue;
                            }

                            int processId = proc.Id; // Store PID before killing
                            Debug.WriteLine($"Killing agent process with PID: {processId}");

                            proc.Kill();
                            proc.WaitForExit(2000); // Wait up to 2 seconds for each process
                            proc.Dispose();

                            Debug.WriteLine($"Successfully terminated agent process with PID: {processId}");
                        }
                        catch (InvalidOperationException)
                        {
                            // Process has already exited or is no longer accessible
                            Debug.WriteLine($"Process already exited or inaccessible, skipping.");
                            try { proc.Dispose(); } catch { }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Failed to kill process: {ex.Message}");
                            try { proc.Dispose(); } catch { }
                        }
                    }

                    // wait for all processes to fully terminate
                    System.Threading.Thread.Sleep(500);
                    Debug.WriteLine("All existing agent processes terminated.");
                }
                else
                {
                    Debug.WriteLine("No existing agent processes found.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking for existing agent processes: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops the agent process
        /// </summary>
        private void StopAgentProcess()
        {
            try
            {
                if (agentProcess != null && !agentProcess.HasExited)
                {
                    Debug.WriteLine("Stopping agent process...");
                    agentProcess.Kill();
                    agentProcess.WaitForExit(5000); // Wait up to 5 seconds
                    agentProcess.Dispose();
                    agentProcess = null;
                    Debug.WriteLine("Agent process stopped successfully.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping agent process: {ex}");
            }
        }

        /// <summary>
        /// Restarts the agent process
        /// </summary>
        private void RestartAgentProcess()
        {
            Debug.WriteLine("Restarting agent process...");

            // Stop our tracked agent process
            StopAgentProcess();

            // Also kill any other agent processes that might be running
            KillExistingAgentProcesses();

            System.Threading.Thread.Sleep(500); // Give it a moment to fully terminate
            StartAgentProcess();
        }
        #endregion

        #region Tray Icon Management
        /// <summary>
        /// Initializes the system tray icon and context menu
        /// </summary>
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

        /// <summary>
        /// Event handler for "Open UI" menu item
        /// </summary>
        private void OnOpenUI(object sender, EventArgs e)
        {
            ShowUI();
        }

        /// <summary>
        /// Event handler for "Restart Agent" menu item
        /// </summary>
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

        /// <summary>
        /// Event handler for "Exit" menu item
        /// </summary>
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

        /// <summary>
        /// Shows the UI window
        /// </summary>
        private void ShowUI()
        {
            this.ShowInTaskbar = true; // before showing the windowto prevent default icon
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate();

            // Reapply title bar theme when showing from tray
            if (mavcSave != null)
            {
                SetTitleBarTheme(mavcSave.darkMode);
            }
        }
        #endregion

        #region Form Event Handlers
        /// <summary>
        /// Override form closing to minimize to tray instead of closing if toggle is enabled
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && closeActionToggle.Checked)
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
        private void checkForUpdate()
        {
            Version latestGitHubVersion = null;
            Version localVersion = null;
            try
            {
                Debug.WriteLine("Checking for latest version available...");

                GitHubClient client = new GitHubClient(new ProductHeaderValue("MavcAutoUpdater"));
                IReadOnlyList<Release> releases = Task.Run(() => client.Repository.Release.GetAll("DavidGitter", "multi-app-volume-control")).GetAwaiter().GetResult();

                Debug.WriteLine("Latest Release Tag found: " + releases[0].TagName);

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

        private void updateTimer_Tick(object sender, EventArgs e)  //run this logic each timer tick
        {

            //check for new audio outputs
            if (updateUIFlag.Value == true)
            {
                updateUIFlag.Value = false;
                refreshAvailableOutputs();
                loadFromMavcSave();
            }

        }
        #endregion

        #region Available Outputs Management
        /**
         * Initializes availavleOutput Comboboxes gui after a new load
         * to prevent showing a available output which is already in a volumeList (a.k.a doubling)
         * 
         * @param availableOutputs  a array of available outputs
         */
        private void initAvailableOutputs(AudioOutput[] availableOutputs)
        {
            foreach (var output in availableOutputs)
            {
                if (!confHasAudioOutput(output))
                {
                    AddVol1.Items.Add(output);
                    AddVol2.Items.Add(output);
                    AddVol3.Items.Add(output);
                    AddVol4.Items.Add(output);
                }
            }

            //add additional functions
            AudioFocused af = new AudioFocused(audioController);
            AddVol1.Items.Add(af);
            AddVol2.Items.Add(af);
            AddVol3.Items.Add(af);
            AddVol4.Items.Add(af);

            AudioOtherApps aoa = new AudioOtherApps(audioController, mavcSave);
            AddVol1.Items.Add(aoa);
            AddVol2.Items.Add(aoa);
            AddVol3.Items.Add(aoa);
            AddVol4.Items.Add(aoa);
        }

        /** 
         * Refreshes the available outputs
         */
        private void refreshAvailableOutputs()
        {
            availableOutputs.Clear();
            removeAvailableOutputs();
            availableOutputs = audioController.GetAllAudioOutputs();
            initAvailableOutputs(availableOutputs.ToArray());
        }

        /**
         * Removes a item of the avail. outp. comboboxes
         * 
         * @param output    the audio output to be removed
         */
        private void removeAvailableOutput(AudioOutput output)
        {

            AddVol1.Items.Remove(output);
            AddVol2.Items.Remove(output);
            AddVol3.Items.Remove(output);
            AddVol4.Items.Remove(output);
        }

        /**
         * Removes all items of the avail. outp. comboboxes
         */
        private void removeAvailableOutputs()
        {

            AddVol1.Items.Clear();
            AddVol2.Items.Clear();
            AddVol3.Items.Clear();
            AddVol4.Items.Clear();
        }

        /** adds a audio output to the avail. outp. comboboxes
         * 
         * @param output    the output to be added to the combobox
         */
        private void addAvailableOutput(AudioOutput output)
        {

            AddVol1.Items.Add(output);
            AddVol2.Items.Add(output);
            AddVol3.Items.Add(output);
            AddVol4.Items.Add(output);
        }

        /**
         * Retrieves if the conf storage has a specified audio output
         * 
         * @param ao    the audio output to be checked for its existence
         */
        private bool confHasAudioOutput(AudioOutput ao)
        {

            return mavcSave.AOsVol1.Exists(mavc_ao => ao.GetName().Equals(mavc_ao.name)) ||
                   mavcSave.AOsVol2.Exists(mavc_ao => ao.GetName().Equals(mavc_ao.name)) ||
                   mavcSave.AOsVol3.Exists(mavc_ao => ao.GetName().Equals(mavc_ao.name)) ||
                   mavcSave.AOsVol4.Exists(mavc_ao => ao.GetName().Equals(mavc_ao.name));
        }
        #endregion

        #region Configuration Management
        /**
         * Eventhandler of the gui "Save" button
         */
        private void saveBtn_Click(object sender, EventArgs e)
        {
            save(configSavePath, configFileName);
            RestartAgentProcess();
        }

        /**
         * Updated the config save member with the newest listbox output selections
         */
        private void updateMavcSave()
        {
            mavcSave.AOsVol1.Clear();
            mavcSave.AOsVol2.Clear();
            mavcSave.AOsVol3.Clear();
            mavcSave.AOsVol4.Clear();

            foreach (AudioOutput ao in VolList1.Items) //TODO: handle apps that are online while mapping and then offline when saving
                mavcSave.AOsVol1.Add(new MAVCSave.AudioOutput(ao.GetName(), ao.GetAudioType()));
            foreach (AudioOutput ao in VolList2.Items)
                mavcSave.AOsVol2.Add(new MAVCSave.AudioOutput(ao.GetName(), ao.GetAudioType()));
            foreach (AudioOutput ao in VolList3.Items)
                mavcSave.AOsVol3.Add(new MAVCSave.AudioOutput(ao.GetName(), ao.GetAudioType()));
            foreach (AudioOutput ao in VolList4.Items)
                mavcSave.AOsVol4.Add(new MAVCSave.AudioOutput(ao.GetName(), ao.GetAudioType()));
        }

        /**
         * Loads the stored audio outputs form the config save memeber to the listBoxes
         */
        private void loadFromMavcSave()
        {
            ClearVolLists();

            try
            {
                Task t1;
                Task t2;
                Task t3;
                Task t4;

                var foundAudioOutputs1 = new List<AudioOutput>();
                t1 = Task.Run(() =>
                {
                    foreach (MAVCSave.AudioOutput mavc_ao in mavcSave.AOsVol1)
                        try
                        {
                            if (!mavc_ao.type.Equals("Function"))
                                foundAudioOutputs1.Add(audioController.GetOutputByName(mavc_ao.name));
                            else
                                if (mavc_ao.name.Equals("Focused"))
                                foundAudioOutputs1.Add(new AudioFocused(audioController));
                            else if (mavc_ao.name.Equals("Other Apps"))
                                foundAudioOutputs1.Add(new AudioOtherApps(audioController, mavcSave));
                            else
                                throw new NotImplementedException();
                        }
                        catch (Exception knfe)
                        {
                            // Add Log / Debug
                            Console.WriteLine("AudioOutput " + mavc_ao.name + " of mavc save not found");
                            foundAudioOutputs1.Add(new AudioOutputOffline(mavc_ao.name));
                        }
                });

                var foundAudioOutputs2 = new List<AudioOutput>();
                t2 = Task.Run(() =>
                {
                    foreach (MAVCSave.AudioOutput mavc_ao in mavcSave.AOsVol2)
                        try
                        {
                            if (!mavc_ao.type.Equals("Function"))
                                foundAudioOutputs2.Add(audioController.GetOutputByName(mavc_ao.name));
                            else
                                if (mavc_ao.name.Equals("Focused"))
                                foundAudioOutputs2.Add(new AudioFocused(audioController));
                            else if (mavc_ao.name.Equals("Other Apps"))
                                foundAudioOutputs2.Add(new AudioOtherApps(audioController, mavcSave));
                            else
                                throw new NotImplementedException();
                        }
                        catch (Exception knfe)
                        {
                            // Add Log / Debug
                            Console.WriteLine("AudioOutput " + mavc_ao.name + " of mavc save not found");
                            foundAudioOutputs2.Add(new AudioOutputOffline(mavc_ao.name));
                        }
                });



                var foundAudioOutputs3 = new List<AudioOutput>();
                t3 = Task.Run(() =>
                {
                    foreach (MAVCSave.AudioOutput mavc_ao in mavcSave.AOsVol3)
                        try
                        {
                            if (!mavc_ao.type.Equals("Function"))
                                foundAudioOutputs3.Add(audioController.GetOutputByName(mavc_ao.name));
                            else
                                if (mavc_ao.name.Equals("Focused"))
                                foundAudioOutputs3.Add(new AudioFocused(audioController));
                            else if (mavc_ao.name.Equals("Other Apps"))
                                foundAudioOutputs3.Add(new AudioOtherApps(audioController, mavcSave));
                            else
                                throw new NotImplementedException();
                        }
                        catch (Exception knfe)
                        {
                            // Add Log / Debug
                            Console.WriteLine("AudioOutput " + mavc_ao + " of mavc save not found");
                            foundAudioOutputs3.Add(new AudioOutputOffline(mavc_ao.name));
                        }
                });


                var foundAudioOutputs4 = new List<AudioOutput>();
                t4 = Task.Run(() =>
                {
                    foreach (MAVCSave.AudioOutput mavc_ao in mavcSave.AOsVol4)
                        try
                        {
                            if (!mavc_ao.type.Equals("Function"))
                                foundAudioOutputs4.Add(audioController.GetOutputByName(mavc_ao.name));
                            else
                                if (mavc_ao.name.Equals("Focused"))
                                foundAudioOutputs4.Add(new AudioFocused(audioController));
                            else if (mavc_ao.name.Equals("Other Apps"))
                                foundAudioOutputs4.Add(new AudioOtherApps(audioController, mavcSave));
                            else
                                throw new NotImplementedException();
                        }
                        catch (Exception knfe)
                        {
                            // Add Log / Debug
                            Console.WriteLine("AudioOutput " + mavc_ao + " of mavc save not found");
                            foundAudioOutputs4.Add(new AudioOutputOffline(mavc_ao.name));
                        }
                });

                Task.WaitAll(t1, t2, t3, t4);
                VolList1.Items.AddRange(foundAudioOutputs1.ToArray());
                VolList2.Items.AddRange(foundAudioOutputs2.ToArray());
                VolList3.Items.AddRange(foundAudioOutputs3.ToArray());
                VolList4.Items.AddRange(foundAudioOutputs4.ToArray());

                // update knob-reversed checkboxes
                reverseCheckbox1.Checked = mavcSave.reverseKnob1;
                reverseCheckbox2.Checked = mavcSave.reverseKnob2;
                reverseCheckbox3.Checked = mavcSave.reverseKnob3;
                reverseCheckbox4.Checked = mavcSave.reverseKnob4;

                //update knob order
                reverseKnobsCheckbox.Checked = mavcSave.reverseKnobOrder;

                // load darkmode state
                darkModeToolStripMenuItem.Checked = mavcSave.darkMode;
                ApplyTheme(mavcSave.darkMode);

                // load minimize on close setting
                closeActionToggle.Checked = mavcSave.minimizeOnClose;

                // update enable debug mode
                enableDebugBox.Checked = mavcSave.enableDebugMode;

                // load start minimized setting
                startMinimized.Checked = mavcSave.startMinimized;

                // update box for screen overlay
                toolStripMenuItemOverlay.Checked = mavcSave.enableScreenOverlay;

                // update auto hide active checkbox item
                activeAutoHideToolStripMenuItem.Checked = mavcSave.activateAutoHide;

                // update auto hide after seconds textbox
                autoHideAfterSectoolStripTextBox.Text = mavcSave.autoHideAfterSec.ToString();

                // update overlay position numeric updowns
                numericUpDown1.Value = mavcSave.overlayX;
                numericUpDown2.Value = mavcSave.overlayY;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                Console.WriteLine("Config file cannot be opened or is invalid - creating new one...");

                save(configSavePath, configFileName);
            }

        }

        /**
         * Saves the config saves to a specified file
         *
         * @param path  path to the save file (the folder)
         * @param file  the name of the file
         */
        private void save(string path, string file)
        {
            updateMavcSave();
            if (!System.IO.File.Exists(Path.Combine(path, file)))
            {
                Directory.CreateDirectory(path);
                System.IO.File.Create(file);
            }

            // Serialize the class to JSON
            string json = JsonConvert.SerializeObject(mavcSave);

            // Save the JSON string to a file
            System.IO.File.WriteAllText(configFilePath, json);
        }

        /**
         * The event handler of the menu bar save button
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

        /**
         * Saves the config saves to a choosable path in the gui with a chooser (for backuping)
         */
        private void saveTo()
        {
            string selectedFilePath = null;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "MAVC Config File|*.mavc";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFilePath = saveFileDialog.FileName;
                Console.WriteLine("Selected file: " + selectedFilePath);
            }

            save(selectedFilePath, configFileName);
        }

        /**
         * Event Handler that gets called by the gui "Save To" menu bar button
         */
        private void SaveToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveTo();
        }

        /**
         * Loads a config save from a file by a specified path
         *
         * @param configFileFolder  path to the save file (the folder)
         * @param configFileName    the name of the file
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
                Console.WriteLine("Config file " + configFilePath + " propably not existing, creating new one...");
                logger.Warning("Config file " + configFilePath + " propably not existing, creating new one...");
                save(configSavePath, configFileName);

            }

            loadFromMavcSave();
            availableOutputs = audioController.GetAllAudioOutputs();
            initAvailableOutputs(availableOutputs.ToArray());
        }

        /**
         * Event Handler that gets called by the gui "Open" menu bar button to open a config file
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
                    Console.WriteLine("Selected file: " + selectedFilePath);
                }

                //TODO: check if config is valid otherwise abort load

                ClearVolLists();
                loadConfig(Path.GetDirectoryName(selectedFilePath), Path.GetFileName(selectedFilePath));
            }
            else
            {
                // stop opening
                Console.WriteLine("User clicked Yes.");
            }
        }
        #endregion

        #region Volume List Event Handlers
        /**
         * Eventhandler of the Volume 1 Combobox  (available audio outputs)
         */
        private void AddVol1_SelectedIndexChanged(object sender, EventArgs e)
        {
            AudioOutput selectedAO = (AudioOutput)AddVol1.SelectedItem;
            VolList1.Items.Add(selectedAO);
            removeAvailableOutput(selectedAO);
            //AddVol1.DroppedDown = true;
        }

        /**
       * Eventhandler of the Volume 2 Combobox  (available audio outputs)
       */
        private void AddVol2_SelectedIndexChanged(object sender, EventArgs e)
        {
            AudioOutput selectedAO = (AudioOutput)AddVol2.SelectedItem;
            VolList2.Items.Add(selectedAO);
            removeAvailableOutput(selectedAO);
        }

        /**
       * Eventhandler of the Volume 3 Combobox  (available audio outputs)
       */
        private void AddVol3_SelectedIndexChanged(object sender, EventArgs e)
        {
            AudioOutput selectedAO = (AudioOutput)AddVol3.SelectedItem;
            VolList3.Items.Add(selectedAO);
            removeAvailableOutput(selectedAO);
        }

        /**
       * Eventhandler of the Volume 4 Combobox (available audio outputs)
       */
        private void AddVol4_SelectedIndexChanged(object sender, EventArgs e)
        {
            AudioOutput selectedAO = (AudioOutput)AddVol4.SelectedItem;
            VolList4.Items.Add(selectedAO);
            removeAvailableOutput(selectedAO);
        }

        /**
         * The event handler for the gui "Delete Selection" button to delete a audio output in a listbox
         */
        private void delItemBtn_Click(object sender, EventArgs e)
        {
            List<AudioOutput> selectedItems = new List<AudioOutput>();

            foreach (AudioOutput ao in VolList1.SelectedItems)
                selectedItems.Add(ao);
            foreach (AudioOutput ao in VolList2.SelectedItems)
                selectedItems.Add(ao);
            foreach (AudioOutput ao in VolList3.SelectedItems)
                selectedItems.Add(ao);
            foreach (AudioOutput ao in VolList4.SelectedItems)
                selectedItems.Add(ao);

            foreach (AudioOutput ao in selectedItems)
            {
                VolList1.Items.Remove(ao);
                VolList2.Items.Remove(ao);
                VolList3.Items.Remove(ao);
                VolList4.Items.Remove(ao);
                addAvailableOutput(ao);
            }
        }

        /**
         * Discards all selected audio outputs in the list boxes
         */
        private void discSelBtn_Click(object sender, EventArgs e)
        {
            VolList1.ClearSelected();
            VolList2.ClearSelected();
            VolList3.ClearSelected();
            VolList4.ClearSelected();
        }

        /**
         * Event Handler that gets called by the gui "Save To" menu bar button
         */
        private void ClearVolLists()
        {
            VolList1.Items.Clear();
            VolList2.Items.Clear();
            VolList3.Items.Clear();
            VolList4.Items.Clear();
        }
        #endregion

        #region UI Control Event Handlers
        private void reverseCheckbox1_CheckedChanged(object sender, EventArgs e)
        {
            mavcSave.reverseKnob1 = reverseCheckbox1.Checked;
        }

        private void reverseCheckbox2_CheckedChanged(object sender, EventArgs e)
        {
            mavcSave.reverseKnob2 = reverseCheckbox2.Checked;
        }

        private void reverseCheckbox3_CheckedChanged(object sender, EventArgs e)
        {
            mavcSave.reverseKnob3 = reverseCheckbox3.Checked;
        }

        private void reverseCheckbox4_CheckedChanged(object sender, EventArgs e)
        {
            mavcSave.reverseKnob4 = reverseCheckbox4.Checked;
        }

        private void reverseKnobsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            mavcSave.reverseKnobOrder = reverseKnobsCheckbox.Checked;
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //refresh all audio outputs available + there state
            refreshAvailableOutputs();
            loadFromMavcSave();
        }

        private void darkModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mavcSave.darkMode = !mavcSave.darkMode; // toggle
            ApplyTheme(mavcSave.darkMode);          // refresh
            save(configSavePath, configFileName);   // save
        }

        private void ApplyTheme(bool isDark)
        {
            ThemeColors.ApplyTheme(this, isDark);
            darkModeToolStripMenuItem.Checked = isDark;
        }

        private void enableDebugBox_CheckedChanged(object sender, EventArgs e)
        {
            mavcSave.enableDebugMode = enableDebugBox.Checked;
        }

        private void closeActionToggle_CheckedChanged(object sender, EventArgs e)
        {
            mavcSave.minimizeOnClose = closeActionToggle.Checked;
        }

        private void startMinimized_CheckedChanged(object sender, EventArgs e)
        {
            mavcSave.startMinimized = startMinimized.Checked;
        }

        private void toolStripMenuItemOverlay_Click(object sender, EventArgs e)
        {
            mavcSave.enableScreenOverlay = toolStripMenuItemOverlay.Checked;
            save(configSavePath, configFileName);
            Debug.WriteLine("checked: " + mavcSave.enableScreenOverlay);
        }

        private void activeAutoHideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mavcSave.activateAutoHide = activeAutoHideToolStripMenuItem.Checked;
        }
        #endregion

        #region Miscellaneous Event Handlers
        /**
        * Event Handler of the gui "Help" menu button
        */
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}