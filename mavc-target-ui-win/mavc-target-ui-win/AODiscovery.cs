using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace mavc_target_ui_win
{
    // a simple store which eliminates duplicates and stores discovered ao names
    internal class AODiscovery
    {
        private AudioController audioController = null;
        private HashSet<string> foundAONames; // this list includes all ao entity names that were discovered

        // found location: %USERPROFILE%\Documents\MAVC\discovered.txt
        private string discoveredSavePath = null;
        private string discoveredFileName = null;
        private string discoveredFilePath = null;

        public AODiscovery(AudioController audioController) : this(audioController, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MAVC"), "discovered.txt")
        {
        }

        public AODiscovery(AudioController audioController, String discoveredSavePath, String discoveredFileName) {
            this.audioController = audioController;
            this.foundAONames = new HashSet<string>();

            this.discoveredSavePath = discoveredSavePath;
            this.discoveredFileName = discoveredFileName;
            this.discoveredFilePath = Path.Combine(discoveredSavePath, discoveredFileName);

            if (!File.Exists(discoveredFilePath))
            {
                Console.WriteLine("Could not find the discovery save file " + discoveredFileName + ". Creating new file.");
                File.CreateText(discoveredFilePath);
                // TODO: Call logger here
            }

            foundAONames = File.ReadAllLines(discoveredFilePath).ToList().ToHashSet();
        }

        /**
         * Returns a list of all known offline and online audio outputs (found by scanning the system + found older ones saved in the discovery file) without duplicates
         * 
         * @returns all found audio outputs
         */
        public List<AudioOutput> GetAllAudioOutputs()
        {
            discoverAOs();
            List<AudioOutput> found = new List<AudioOutput>();
            foreach(string aoname in foundAONames) {
                List<AudioOutput> avail = audioController.GetOutputsByName(aoname);
                if(avail.Count == 0)
                {
                    found.Add(new AudioOutputOffline(aoname));
                }
                else
                {
                    found.AddRange(avail);
                }
            }

            return found;
        }

        /**
         * Updates the foundAO List
         * 
         * @returns all found audio outputs
         */
        private void discoverAOs()
        {
            List<string> foundOnSystem = audioController.GetAllAudioOutputs().Select(obj => obj.GetName()).ToList();
            foundAONames.UnionWith(foundOnSystem);
            updateDiscoveredFile(foundAONames.ToList());
        }

        /**
         * Updates the discovered file
         * 
         * @returns all found audio outputs
         */
        private void updateDiscoveredFile(List<string> aoNames)
        {
            File.WriteAllLines(discoveredFilePath, aoNames);
        }
    }
}
