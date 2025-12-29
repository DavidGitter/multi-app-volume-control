using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
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
        private string CURRENT_VERSION = "1.1.1";

        private AudioController audioController;
        private string configSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MAVC");
        private string configFileName = "config.json";
        private string configFilePath;

        private List<AudioOutput> availableOutputs;
        private MAVCSave mavcSave;

        // general purpose timer for updating etc.
        Timer updateTimer = new Timer();

        // for notifying if there is a ui update
        ThreadSafeBool updateUIFlag = new ThreadSafeBool();

        Log logger = new Log(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"MAVC", "ui-log.txt"));

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private void SetTitleBarTheme(bool isDark)
        {
            int darkMode = isDark ? 1 : 0;
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
        }

        public Form1()
        {
            try
            {
                //load list of apps and devices
                InitializeComponent();

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
                configFilePath = Path.Combine(configSavePath, configFileName);

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
            }
            catch (Exception e){
                logger.Error(e.ToString());
            }
        }

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
            WebClient webClient = new WebClient();
            var client = new WebClient();

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
                catch(Exception ex)
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

        /**
         * Initializes availavleOutput Comboboxes gui after a new load
         * to prevent showing a available output which is already in a volumeList (a.k.a doubling)
         * 
         * @param availableOutputs  a array of available outputs
         */
        private void initAvailableOutputs(AudioOutput[] availableOutputs)
        {
            foreach(var output in availableOutputs)
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

        /**
         * Eventhandler of the gui "Save" button
         */
        private void saveBtn_Click(object sender, EventArgs e)
        {
            save(configSavePath, configFileName);
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

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
                foreach (MAVCSave.AudioOutput mavc_ao in mavcSave.AOsVol1)
                    try
                    {
                        if (!mavc_ao.type.Equals("Function"))
                            VolList1.Items.Add(audioController.GetOutputByName(mavc_ao.name));
                        else
                            if (mavc_ao.name.Equals("Focused"))
                                VolList1.Items.Add(new AudioFocused(audioController));
                            else if (mavc_ao.name.Equals("Other Apps"))
                                VolList1.Items.Add(new AudioOtherApps(audioController, mavcSave));
                        else
                            throw new NotImplementedException();
                    }
                    catch (Exception knfe)
                    {
                        // Add Log / Debug
                        Console.WriteLine("AudioOutput " + mavc_ao.name + " of mavc save not found");
                        VolList1.Items.Add(new AudioOutputOffline(mavc_ao.name));
                    }

                foreach (MAVCSave.AudioOutput mavc_ao in mavcSave.AOsVol2)
                    try
                    {
                        if (!mavc_ao.type.Equals("Function"))
                            VolList2.Items.Add(audioController.GetOutputByName(mavc_ao.name));
                        else
                            if (mavc_ao.name.Equals("Focused"))
                                VolList2.Items.Add(new AudioFocused(audioController));
                            else if (mavc_ao.name.Equals("Other Apps"))
                                VolList2.Items.Add(new AudioOtherApps(audioController, mavcSave));
                        else
                            throw new NotImplementedException();
                    }
                    catch (Exception knfe)
                    {
                        // Add Log / Debug
                        Console.WriteLine("AudioOutput " + mavc_ao.name + " of mavc save not found");
                        VolList2.Items.Add(new AudioOutputOffline(mavc_ao.name));
                    }

                foreach (MAVCSave.AudioOutput mavc_ao in mavcSave.AOsVol3)
                    try
                    {
                        if (!mavc_ao.type.Equals("Function"))
                            VolList3.Items.Add(audioController.GetOutputByName(mavc_ao.name));
                        else
                            if (mavc_ao.name.Equals("Focused"))
                                VolList3.Items.Add(new AudioFocused(audioController));
                            else if (mavc_ao.name.Equals("Other Apps"))
                                VolList3.Items.Add(new AudioOtherApps(audioController, mavcSave));
                        else
                            throw new NotImplementedException();
                    }
                    catch (Exception knfe)
                    {
                        // Add Log / Debug
                        Console.WriteLine("AudioOutput " + mavc_ao + " of mavc save not found");
                        VolList3.Items.Add(new AudioOutputOffline(mavc_ao.name));
                    }

                foreach (MAVCSave.AudioOutput mavc_ao in mavcSave.AOsVol4)
                    try
                    {
                        if (!mavc_ao.type.Equals("Function"))
                            VolList4.Items.Add(audioController.GetOutputByName(mavc_ao.name));
                        else
                            if (mavc_ao.name.Equals("Focused"))
                                VolList4.Items.Add(new AudioFocused(audioController));
                            else if (mavc_ao.name.Equals("Other Apps"))
                                VolList4.Items.Add(new AudioOtherApps(audioController, mavcSave));
                        else
                            throw new NotImplementedException();
                    }
                    catch (Exception knfe)
                    {
                        // Add Log / Debug
                        Console.WriteLine("AudioOutput " + mavc_ao + " of mavc save not found");
                        VolList4.Items.Add(new AudioOutputOffline(mavc_ao.name));
                    }


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

                // update enable debug mode
                enableDebugBox.Checked = mavcSave.enableDebugMode;

            }catch(Exception e){
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                Console.WriteLine("Config file cannot be opened or is invalid - creating new one...");

                save(configSavePath, configFileName);
            }

        }

        /**
         * The event handler for the gui "Delete Selection" button to delete a audio output in a listbox
         */
        private void delItemBtn_Click(object sender, EventArgs e)
        {
            List<AudioOutput> selectedItems = new List<AudioOutput>();

            foreach(AudioOutput ao in VolList1.SelectedItems)
                selectedItems.Add(ao);
            foreach (AudioOutput ao in VolList2.SelectedItems)
                selectedItems.Add(ao);
            foreach (AudioOutput ao in VolList3.SelectedItems)
                selectedItems.Add(ao);
            foreach (AudioOutput ao in VolList4.SelectedItems)
                selectedItems.Add(ao);

            foreach (AudioOutput ao in selectedItems) {
                VolList1.Items.Remove(ao);
                VolList2.Items.Remove(ao);
                VolList3.Items.Remove(ao);
                VolList4.Items.Remove(ao);
                addAvailableOutput(ao);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

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
        * Event Handler of the gui "Help" menu button
        */
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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
            if(configSavePath == null)
            {
                saveTo();
            }
            else
            {
                save(configSavePath, configFileName);
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
         * Event Handler that gets called by the gui "Save To" menu bar button
         */
        private void ClearVolLists()
        {
            VolList1.Items.Clear();
            VolList2.Items.Clear();
            VolList3.Items.Clear();
            VolList4.Items.Clear();
        }

        /**
         * Loads a config save from a file by a specified path
         *
         * @param configFileFolder  path to the save file (the folder)
         * @param configFileName    the name of the file
         */
        private void loadConfig(string configFileFolder, string configFileName)
        {
            string configFilePath = Path.Combine(configFileFolder, configFileName);
            if (System.IO.File.Exists(configFilePath))
            {
                string json = System.IO.File.ReadAllText(configFilePath);
                mavcSave = JsonConvert.DeserializeObject<MAVCSave>(json);
                loadFromMavcSave();
            }
            else
            {
                save(configSavePath, configFileName);
            }

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

        private void VolList4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }

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
            Color backColor = ThemeColors.GetBackground(isDark);
            Color textColor = ThemeColors.GetText(isDark);
            Color listBackColor = ThemeColors.GetListBackground(isDark);
            Color buttonBackColor = ThemeColors.GetButtonBackground(isDark);
            Color menuBackColor = ThemeColors.GetMenuBackground(isDark);
            Color borderColor = ThemeColors.GetBorder(isDark);

            SetTitleBarTheme(isDark);

            this.BackColor = backColor;
            
            foreach (Control topControl in this.Controls)
            {
                if (topControl is MenuStrip menuStrip)
                {
                    menuStrip.BackColor = menuBackColor;
                    menuStrip.ForeColor = textColor;
                    menuStrip.Renderer = new ToolStripProfessionalRenderer(new DiscordColorTable(isDark));
                    foreach (ToolStripMenuItem menuItem in menuStrip.Items)
                    {
                        ApplyThemeToMenuItem(menuItem, menuBackColor, textColor);
                    }
                }
            }
            
            UpdateControlTheme(this, backColor, textColor, listBackColor, buttonBackColor, borderColor, isDark);

            darkModeToolStripMenuItem.Checked = isDark;
        }

        private void ApplyThemeToMenuItem(ToolStripMenuItem item, Color backColor, Color textColor)
        {
            item.BackColor = backColor;
            item.ForeColor = textColor;
            foreach (ToolStripItem subItem in item.DropDownItems)
            {
                subItem.BackColor = backColor;
                subItem.ForeColor = textColor;
                if (subItem is ToolStripMenuItem subMenuItem)
                {
                    ApplyThemeToMenuItem(subMenuItem, backColor, textColor);
                }
            }
        }

        private void UpdateControlTheme(Control parent, Color back, Color text, Color listBack, Color buttonBack, Color border, bool isDark)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is MenuStrip)
                {
                    continue;
                }
                else if (c is ComboBox combo)
                {
                    combo.BackColor = listBack;
                    combo.ForeColor = text;
                    combo.FlatStyle = isDark ? FlatStyle.Flat : FlatStyle.Standard;
                }
                else if (c is ListBox)
                {
                    c.BackColor = listBack;
                    c.ForeColor = text;
                }
                else if (c is TextBox txt)
                {
                    txt.BackColor = listBack;
                    txt.ForeColor = text;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is Button btn)
                {
                    btn.BackColor = buttonBack;
                    btn.ForeColor = text;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = border;
                }
                else if (c is CheckBox || c is System.Windows.Forms.Label)
                {
                    c.ForeColor = text;
                }
                else if (c is GroupBox gb)
                {
                    gb.ForeColor = text;
                    gb.FlatStyle = FlatStyle.Flat;
                }
                else if (c is Panel || c is TabControl || c is TabPage)
                {
                    c.BackColor = back;
                    c.ForeColor = text;
                }

                if (c.HasChildren) UpdateControlTheme(c, back, text, listBack, buttonBack, border, isDark);
            }
        }

        private class DiscordColorTable : ProfessionalColorTable
        {
            private bool isDark;

            public DiscordColorTable(bool isDarkMode)
            {
                isDark = isDarkMode;
            }

            public override Color MenuItemSelected
            {
                get { return ThemeColors.GetMenuItemSelected(isDark); }
            }

            public override Color MenuItemSelectedGradientBegin
            {
                get { return ThemeColors.GetMenuItemSelected(isDark); }
            }

            public override Color MenuItemSelectedGradientEnd
            {
                get { return ThemeColors.GetMenuItemSelected(isDark); }
            }

            public override Color MenuItemBorder
            {
                get { return ThemeColors.GetBorder(isDark); }
            }

            public override Color MenuBorder
            {
                get { return ThemeColors.GetBorder(isDark); }
            }

            public override Color MenuItemPressedGradientBegin
            {
                get { return ThemeColors.GetMenuItemPressed(isDark); }
            }

            public override Color MenuItemPressedGradientEnd
            {
                get { return ThemeColors.GetMenuItemPressed(isDark); }
            }

            public override Color ImageMarginGradientBegin
            {
                get { return ThemeColors.GetMenuBackground(isDark); }
            }

            public override Color ImageMarginGradientMiddle
            {
                get { return ThemeColors.GetMenuBackground(isDark); }
            }

            public override Color ImageMarginGradientEnd
            {
                get { return ThemeColors.GetMenuBackground(isDark); }
            }

            public override Color ToolStripDropDownBackground
            {
                get { return ThemeColors.GetMenuBackground(isDark); }
            }
        }

        private void enableDebugBox_CheckedChanged(object sender, EventArgs e)
        {
            mavcSave.enableDebugMode = enableDebugBox.Checked;
        }
    }
}
