using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ModOrganizerHelper.Properties;

namespace ModOrganizerHelper
{
    public class Worker
    {
        public delegate void LogDelegate(string message, bool replace);

        private readonly ModOrganizerConfig _config;

        public string[] Profiles = Directory.EnumerateDirectories(Path.Combine(Settings.Default.IniPath, "profiles")).Select(Path.GetFileName).ToArray();

        public Worker(string iniPath) {
            _config = new ModOrganizerConfig(iniPath);
        }

        public string ProfileName => _config.SelectedProfile;

        public string GamePath => _config.GamePath;

        public string ProfilePath => Path.Combine(Settings.Default.IniPath, "profiles", ProfileName);

        public event LogDelegate Log;

        public void DeleteAllLinks() {
            Dictionary<string, string> actualLinks = LoadLinkList();
            Dictionary<string, string> diff = actualLinks.ToDictionary(o => o.Key, o => (string) null);
            UpdateLinks(diff);
            DeleteEmptyDirs();
            SaveLinkList(null);
            OnLog("done");
        }

        public void UpdateLinks() {
            if (Settings.Default.ProfileName != ProfileName) {
                LinkSaveDirectory();
                LinkIniFiles();
                Settings.Default.ProfileName = ProfileName;
                Settings.Default.Save();
                Thread.Sleep(100);
            }

            UpdatePlugins();
            string[] modList = LoadMods();
            Dictionary<string, string> actualLinks = ResolveFileLinks(modList);
            Dictionary<string, string> diff = GetFileLinksDiff(actualLinks);
            UpdateLinks(diff);
            DeleteEmptyDirs();
            SaveLinkList(actualLinks);
            OnLog("done");
        }

        private void UpdatePlugins() {
            OnLog("Updating plugins.txt ... ");

            string srcPath = Path.Combine(ProfilePath, "plugins.txt");
            string destPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fallout4", "plugins.txt");

            string[] dlc = {
                "*Fallout4.esm",
                "*DLCWorkshop01.esm",
                "*DLCWorkshop02.esm",
                "*DLCWorkshop03.esm",
                "*DLCCoast.esm",
                "*DLCRobot.esm",
                "*DLCNukaWorld.esm"
            };

            IEnumerable<string> plugins = File.ReadLines(srcPath).Where(o => o[0] != '#' && !dlc.Contains(o));
            File.WriteAllLines(destPath, plugins);
            Thread.Sleep(100);
        }

        private void LinkIniFiles() {
            OnLog("Linking ini files ... ");
            string[] files = {"fallout4.ini", "Fallout4Custom.ini", "fallout4prefs.ini"};

            string documentsConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Fallout4");
            foreach (string file in files) {
                string filePath = Path.Combine(ProfilePath, file);
                string destPath = Path.Combine(documentsConfigPath, file);
                if (File.Exists(destPath)) {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(destPath);
                }

                if (File.Exists(filePath)) {
                    PInvoke.CreateHardLink(destPath, filePath, IntPtr.Zero);
                }
            }

            Thread.Sleep(100);
        }

        private void LinkSaveDirectory() {
            OnLog("Creating save folder link ... ");
            string srcDir = Path.Combine(Directory.GetCurrentDirectory(), "Saves", ProfileName);
            if (!Directory.Exists(srcDir)) {
                OnLog("  source directory not found, creating new one (new profile?) ... ");
                Directory.CreateDirectory(srcDir);
            }

            string documentsConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Fallout4");
            string destDir = Path.Combine(documentsConfigPath, "Saves");
            if (Directory.Exists(destDir)) {
                Directory.Delete(destDir);
            }

            PInvoke.CreateSymbolicLink(destDir, srcDir, PInvoke.SYMBOLIC_LINK_FLAG.DIRECTORY);
            Thread.Sleep(100);
        }

        private void OnLog(string message, bool replace = false) {
            Log?.Invoke(message, replace);
            Application.DoEvents();
        }

        private string[] LoadMods() {
            OnLog($"Loading mod list for profile {ProfileName} ... ");

            string modList = Path.Combine(ProfilePath, "modlist.txt");

            if (!File.Exists(modList)) {
                throw new FileNotFoundException("File 'modlist.txt' was not found in profile directory!");
            }

            IEnumerable<string> lines = File.ReadLines(modList, Encoding.UTF8);
            int allMods = 0;
            IList<string> activeMods = new List<string>();
            foreach (string line in lines) {
                if (line[0] == '#') {
                    continue;
                }

                ++allMods;
                if (line[0] == '+') {
                    activeMods.Add(line.Substring(1));
                }
            }

            string[] result = activeMods.Reverse().ToArray();
            OnLog($"{allMods} mods found out of which {result.Length} are active.");
            Thread.Sleep(100);
            return result;
        }

        private Dictionary<string, string> LoadLinkList() {
            if (string.IsNullOrEmpty(Settings.Default.Links)) {
                return new Dictionary<string, string>();
            }

            return Settings.Default.Links
                .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Split('|'))
                .ToDictionary(o => o[0], o => o[1]);
        }

        private void SaveLinkList(Dictionary<string, string> links) {
            Settings.Default.Links = links != null ? string.Join(Environment.NewLine, links.Select(o => o.Key + "|" + o.Value)) : "";
            Settings.Default.Save();
        }

        private Dictionary<string, string> ResolveFileLinks(string[] modList) {
            OnLog("Listing files for mods ... ");
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (string modName in modList) {
                string modPath = Path.Combine(Settings.Default.IniPath, "mods", modName);
                IEnumerable<string> files = Directory.EnumerateFiles(modPath, "*.*", SearchOption.AllDirectories);
                int counter = 0;
                foreach (string file in files) {
                    string relativePath = file.Substring(modPath.Length + 1);

                    // meta.ini is MO file ...
                    if (relativePath == "meta.ini") {
                        continue;
                    }

                    result[relativePath] = modName;
                    ++counter;
                }

                OnLog($"  {modName} ... {counter} files found", true);
            }

            Thread.Sleep(100);
            return result;
        }

        private Dictionary<string, string> GetFileLinksDiff(Dictionary<string, string> fileLinks) {
            OnLog("Resolving file links ... ");
            Dictionary<string, string> prevFileLinks = LoadLinkList();
            Dictionary<string, string> result = new Dictionary<string, string>();

            int actual = 0;
            int insert = 0;
            int update = 0;
            int delete = 0;

            foreach (KeyValuePair<string, string> link in prevFileLinks) {
                if (fileLinks.TryGetValue(link.Key, out string current)) {
                    if (current == link.Value) {
                        ++actual;
                    } else {
                        ++update;
                        result.Add(link.Key, current);
                    }
                } else {
                    result.Add(link.Key, null);
                    ++delete;
                }
            }

            foreach (KeyValuePair<string, string> link in fileLinks) {
                if (!prevFileLinks.ContainsKey(link.Key)) {
                    result.Add(link.Key, link.Value);
                    ++insert;
                }
            }

            OnLog($"{actual} up to date, {insert} new, {update} changed and {delete} deleted links found.");
            Thread.Sleep(100);
            return result;
        }

        private void UpdateLinks(Dictionary<string, string> diff) {
            OnLog("Updating file links ... ");
            int counter = 0;
            int max = diff.Count - 1;
            foreach (KeyValuePair<string, string> item in diff) {
                ++counter;

                string key = item.Key;
                if (item.Key.Length > 100) {
                    key = item.Key.Substring(0, 50) + "..." + item.Key.Substring(item.Key.Length - 45);
                }

                if (counter % 100 == 0 || counter == max) {
                    OnLog($"{(item.Value != null ? "Updating" : "Deleting")} file link ... {counter} of {diff.Count} ({key}).", true);
                }

                string destPath = Path.Combine(GamePath, "Data", item.Key);
                if (File.Exists(destPath)) {
                    File.Delete(destPath);
                }

                if (item.Value != null) {
                    string srcPath = Path.Combine(Settings.Default.IniPath, "mods", item.Value, item.Key);
                    string dirPath = Path.GetDirectoryName(destPath);
                    Debug.Assert(dirPath != null, nameof(dirPath) + " != null");
                    if (!Directory.Exists(dirPath)) {
                        Directory.CreateDirectory(dirPath);
                    }

                    PInvoke.CreateHardLink(destPath, srcPath, IntPtr.Zero);
                }
            }

            Thread.Sleep(100);
        }

        private void DeleteEmptyDirs() {
            OnLog("Removing empty directories ... ");
            string dataDir = Path.Combine(GamePath, "Data");

            void ProcessDirectory(string path) {
                foreach (string directory in Directory.GetDirectories(path)) {
                    ProcessDirectory(directory);
                    if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0) {
                        OnLog($"Removing empty directories ... ({directory.Substring(dataDir.Length + 1)})", true);
                        Directory.Delete(directory, false);
                    }
                }
            }

            ProcessDirectory(dataDir);

            Thread.Sleep(100);
        }
    }
}
