using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace mavc_target_ui_win
{
    public partial class Form1 : Form
    {
        private AudioController audioController;
        private string configSavePath = null;

        List<AudioOutput> availableOutputs;

        class AudioMappingState
        {
            public AudioMappingState()
            {
                vol1 = new List<AudioOutput>();
                vol2 = new List<AudioOutput>();
                vol3 = new List<AudioOutput>();
                vol4 = new List<AudioOutput>();
            }
            public List<AudioOutput> vol1;
            public List<AudioOutput> vol2;
            public List<AudioOutput> vol3;
            public List<AudioOutput> vol4;
        };

        AudioMappingState mappingState;
        MAVCSave mavcSave;

        public Form1()
        {
            //load list of apps and devices
            InitializeComponent();

            audioController = new AudioController();

            availableOutputs = audioController.GetAllAudioOutputs();
            mappingState = new AudioMappingState();
            mavcSave = new MAVCSave();  
            updateForm();
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void AddVol1_SelectedIndexChanged(object sender, EventArgs e)
        {
            AudioOutput selectedAO = (AudioOutput)AddVol1.SelectedItem;
            mappingState.vol1.Add(selectedAO);
            availableOutputs.Remove(selectedAO);
            AddVol1.DroppedDown = true;
            updateForm();
        }

        private void AddVol2_SelectedIndexChanged(object sender, EventArgs e)
        {
            AudioOutput selectedAO = (AudioOutput)AddVol2.SelectedItem;
            mappingState.vol2.Add(selectedAO);
            availableOutputs.Remove(selectedAO);
            AddVol2.DroppedDown = true;
            updateForm();
        }

        private void AddVol3_SelectedIndexChanged(object sender, EventArgs e)
        {
            AudioOutput selectedAO = (AudioOutput)AddVol3.SelectedItem;
            mappingState.vol3.Add(selectedAO);
            availableOutputs.Remove(selectedAO);
            AddVol3.DroppedDown = true;
            updateForm();
        }

        private void AddVol4_SelectedIndexChanged(object sender, EventArgs e)
        {
            AudioOutput selectedAO = (AudioOutput)AddVol4.SelectedItem;
            mappingState.vol4.Add(selectedAO);
            availableOutputs.Remove(selectedAO);
            AddVol4.DroppedDown = true;
            updateForm();
        }

        //private void VolList1_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Delete)
        //    {
        //        VolList1.Items.Add("YEEEEEET");
        //    }
        //    updateForm();
        //}

        private void updateForm()
        {
            AddVol1.Items.Clear();
            foreach (var output in availableOutputs)
            {
                AddVol1.Items.Add(output);
            }

            AddVol2.Items.Clear();
            foreach (var output in availableOutputs)
            {
                AddVol2.Items.Add(output);
            }

            AddVol3.Items.Clear();
            foreach (var output in availableOutputs)
            {
                AddVol3.Items.Add(output);
            }

            AddVol4.Items.Clear();
            foreach (var output in availableOutputs)
            {
                AddVol4.Items.Add(output);
            }


            VolList1.Items.Clear();
            foreach (var output in mappingState.vol1)
                VolList1.Items.Add(output);

            VolList2.Items.Clear();
            foreach (var output in mappingState.vol2)
                VolList2.Items.Add(output);

            VolList3.Items.Clear();
            foreach (var output in mappingState.vol3)
                VolList3.Items.Add(output);

            VolList4.Items.Clear();
            foreach (var output in mappingState.vol4)
                VolList4.Items.Add(output);

            updateMavcSave();
        }

        private void updateMavcSave()
        {
            mavcSave.namesVol1.Clear();
            mavcSave.namesVol2.Clear();
            mavcSave.namesVol3.Clear();
            mavcSave.namesVol4.Clear();

            foreach (AudioOutput ao in mappingState.vol1)
                mavcSave.namesVol1.Add(ao.GetName());
            foreach (AudioOutput ao in mappingState.vol2)
                mavcSave.namesVol2.Add(ao.GetName());
            foreach (AudioOutput ao in mappingState.vol3)
                mavcSave.namesVol3.Add(ao.GetName());
            foreach (AudioOutput ao in mappingState.vol4)
                mavcSave.namesVol4.Add(ao.GetName());
        }

        private void loadFromMavcSave()
        {
            mappingState.vol1.Clear();
            mappingState.vol2.Clear();
            mappingState.vol3.Clear();
            mappingState.vol4.Clear();

            foreach (string name in mavcSave.namesVol1)
                try
                {
                    mappingState.vol1.Add(audioController.GetOutputByName(name));
                }catch(KeyNotFoundException knfe)
                {
                    // Add Log / Debug
                    Console.WriteLine("AudioOutput " + name + " of mavc save not found");
                    mappingState.vol1.Add(new AudioOutputOffline(name));
                }

            foreach (string name in mavcSave.namesVol2)
                try
                {
                    mappingState.vol2.Add(audioController.GetOutputByName(name));
                }
                catch (KeyNotFoundException knfe)
                {
                    // Add Log / Debug
                    Console.WriteLine("AudioOutput " + name + " of mavc save not found");
                    mappingState.vol2.Add(new AudioOutputOffline(name));
                }

            foreach (string name in mavcSave.namesVol3)
                try
                {
                    mappingState.vol3.Add(audioController.GetOutputByName(name));
                }
                catch (KeyNotFoundException knfe)
                {
                    // Add Log / Debug
                    Console.WriteLine("AudioOutput " + name + " of mavc save not found");
                    mappingState.vol3.Add(new AudioOutputOffline(name));
                }

            foreach (string name in mavcSave.namesVol4)
                try
                {
                    mappingState.vol4.Add(audioController.GetOutputByName(name));
                }
                catch (KeyNotFoundException knfe)
                {
                    // Add Log / Debug
                    Console.WriteLine("AudioOutput " + name + " of mavc save not found");
                    mappingState.vol4.Add(new AudioOutputOffline(name));
                }

            updateForm();
        }

        private void delItemBtn_Click(object sender, EventArgs e)
        {
            foreach(var item in VolList1.SelectedItems){
                mappingState.vol1.Remove((AudioOutput)item);
                availableOutputs.Add((AudioOutput)item);
            }
            foreach (var item in VolList2.SelectedItems)
            {
                mappingState.vol2.Remove((AudioOutput)item);
                availableOutputs.Add((AudioOutput)item);
            }
            foreach (var item in VolList3.SelectedItems)
            {
                mappingState.vol3.Remove((AudioOutput)item);
                availableOutputs.Add((AudioOutput)item);
            }
            foreach (var item in VolList4.SelectedItems)
            {
                mappingState.vol4.Remove((AudioOutput)item);
                availableOutputs.Add((AudioOutput)item);
            }
            updateForm();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void discSelBtn_Click(object sender, EventArgs e)
        {
            VolList1.ClearSelected();
            VolList2.ClearSelected();
            VolList3.ClearSelected();
            VolList4.ClearSelected();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void save(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, mavcSave);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(configSavePath == null)
            {
                saveTo();
            }
            else
            {
                save(configSavePath);
            }
        }

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

            save(selectedFilePath);
        }

        private void saveToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveTo();
        }

        private void clearVolLists()
        {
            VolList1.Items.Clear();
            VolList2.Items.Clear();
            VolList3.Items.Clear();
            VolList4.Items.Clear();
        }

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
                openFileDialog.Filter = "MAVC Config File|*.mavc";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = openFileDialog.FileName;
                    Console.WriteLine("Selected file: " + selectedFilePath);
                }

                clearVolLists();

                // loading new mavc config
                using (FileStream fs = new FileStream(selectedFilePath, FileMode.Open))
                {
                    IFormatter formatter = new BinaryFormatter();
                    mavcSave = (MAVCSave)formatter.Deserialize(fs);
                }
                loadFromMavcSave();
            }
            else
            {
                // stop opening
                Console.WriteLine("User clicked Yes.");
            }
        }
    }
}
