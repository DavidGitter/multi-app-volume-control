﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace mavc_target_ui_win
{
    public partial class Form1 : Form
    {
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

        public Form1()
        {
            //load list of apps and devices
            InitializeComponent();

            this.Text = "MAVC";
            this.Icon = new System.Drawing.Icon("../../icon.ico");

            mavcSave = new MAVCSave();
            audioController = new AudioController();
            configFilePath = Path.Combine(configSavePath, configFileName);

            loadConfig(configSavePath, configFileName);

            var devices = audioController.GetAudioDevices();
            foreach(var dev in devices)
            {
                dev.OnOutputCreated((sender, newSession) => {
                    Console.WriteLine("new output registered");
                    updateUIFlag.Value = true;
                });
            }

            foreach(var ou in availableOutputs)
            {
                Console.WriteLine(ou.ToString());
            }

            updateTimer.Interval = 3000;   // milliseconds
            updateTimer.Tick += updateTimer_Tick;  // set handler
            updateTimer.Start();
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
                        VolList1.Items.Add(audioController.GetOutputByName(mavc_ao.name));
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
                        VolList2.Items.Add(audioController.GetOutputByName(mavc_ao.name));
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
                        VolList3.Items.Add(audioController.GetOutputByName(mavc_ao.name));
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
                        VolList4.Items.Add(audioController.GetOutputByName(mavc_ao.name));
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
            if (!File.Exists(Path.Combine(path, file)))
            {
                Directory.CreateDirectory(path);
                File.Create(file);
            }

            // Serialize the class to JSON
            string json = JsonConvert.SerializeObject(mavcSave);

            // Save the JSON string to a file
            File.WriteAllText(configFilePath, json);
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
            if (File.Exists(configFilePath))
            {
                string json = File.ReadAllText(configFilePath);
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
    }
}
