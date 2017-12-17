using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ModOrganizerHelper.Properties;

namespace ModOrganizerHelper
{
    public class Program
    {
        private static ModOrganizerConfig _modOrganizerConfig;
        private static string _modOrganizerDataDir;
        private static string _profilePath;
        private static string _modOrganizerConfigFile;

        public static void Main(string[] args) {
            try {
                CheckArgs(args);
                WriteHeader();
               
                _modOrganizerConfig = new ModOrganizerConfig(_modOrganizerConfigFile);
                _profilePath = Path.Combine(_modOrganizerDataDir, "profiles", _modOrganizerConfig.SelectedProfile);

                Console.Write("Selected profile: ");
                WriteColored(ConsoleColor.Yellow, _modOrganizerConfig.SelectedProfile);
                Console.WriteLine();
                Console.WriteLine();
                if (Settings.Default.ProfileName != _modOrganizerConfig.SelectedProfile) {
                    Settings.Default.ProfileName = _modOrganizerConfig.SelectedProfile;

                    Console.WriteLine("Profile was changed from last run ...");
                    Console.WriteLine();
                    CopyIniFiles();
                    LinkSaveDirectory();
                }

                string[] modList = LoadMods();
                Dictionary<string, string> actualLinks = ResolveFileLinks(modList);
                Dictionary<string, string> diff = GetFileLinksDiff(actualLinks);
                UpdateLinks(diff);

                Settings.Default.LinkList = string.Join("\r\n", actualLinks.Select(o => o.Key + "|" + o.Value));
                Settings.Default.Save();
                WriteSuccess();
            } catch (Exception ex) {
                WriteError(ex);
            } finally {
                WriteFooter();
            }
        }

        private static void CopyIniFiles() {
            Console.Write("  Copying ini files ... ");
            string[] files = {"fallout4.ini", "Fallout4Custom.ini", "fallout4prefs.ini"};

            string documentsConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Fallout4");
            foreach (string file in files) {
                string filePath = Path.Combine(_profilePath, file);
                string destPath = Path.Combine(documentsConfigPath, file);
                if (File.Exists(destPath)) {
                    File.Delete(destPath);
                }

                if (File.Exists(filePath)) {
                    File.Copy(filePath, destPath);
                }
            }

            WriteColored(ConsoleColor.DarkCyan, "done");
            Console.WriteLine();
        }

        private static Dictionary<string, string> GetFileLinksDiff(Dictionary<string, string> fileLinks) {
            Console.WriteLine();
            Console.Write("Resolving file links ... ");
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

            WriteColored(ConsoleColor.DarkYellow, actual);
            Console.Write(" up to date, ");
            WriteColored(ConsoleColor.DarkYellow, insert);
            Console.Write(" new, ");
            WriteColored(ConsoleColor.DarkYellow, update);
            Console.Write(" changed and ");
            WriteColored(ConsoleColor.DarkYellow, delete);
            Console.WriteLine(" deleted links found.");
            return result;
        }

        private static void CheckArgs(string[] args) {
            if (args.Length != 0) {
                _modOrganizerDataDir = args[0];
                _modOrganizerConfigFile = Path.Combine(_modOrganizerDataDir, "ModOrganizer.ini");
                if (File.Exists(_modOrganizerConfigFile)) {
                    return;
                }
            }

            Console.WriteLine("USAGE: ModOrganizerHelper.exe Path");
            Console.WriteLine();
            Console.WriteLine("  Path: path to mod organizer settings folder (usually at '%LOCALAPPDATA%\\ModOrganizer')");
            Console.WriteLine("        must have 'ModOrganizer.ini' file.");
            Console.WriteLine();
            Environment.Exit(-1);
        }

        private static void LinkSaveDirectory() {
            Console.Write("  Creating save folder link ... ");
            string srcDir = Path.Combine(Directory.GetCurrentDirectory(), "Saves", _modOrganizerConfig.SelectedProfile);
            string documentsConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Fallout4");
            string destDir = Path.Combine(documentsConfigPath, "Saves");
            if (Directory.Exists(destDir)) {
                Directory.Delete(destDir);
            }

            PInvoke.CreateSymbolicLink(destDir, srcDir, PInvoke.SYMBOLIC_LINK_FLAG.DIRECTORY);
            WriteColored(ConsoleColor.DarkCyan, "done");
            Console.WriteLine();
        }

        private static Dictionary<string, string> LoadLinkList() {
            if (string.IsNullOrEmpty(Settings.Default.LinkList)) {
                return new Dictionary<string, string>();
            }

            return Settings.Default.LinkList
                .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Split('|'))
                .ToDictionary(o => o[0], o => o[1]);
        }

        private static string[] LoadMods() {
            Console.WriteLine();
            Console.Write("Loading mod list ... ");
            string modList = Path.Combine(_profilePath, "modlist.txt");

            if (!File.Exists(modList)) {
                WriteColored(ConsoleColor.Red, "File 'modlist.txt' was not found in profile directory!");
                Environment.Exit(-1);
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

            WriteColored(ConsoleColor.DarkYellow, allMods);
            Console.Write(" mods found out of which ");
            WriteColored(ConsoleColor.DarkYellow, result.Length);
            Console.WriteLine(" are active.");

            return result;
        }

        private static Dictionary<string, string> ResolveFileLinks(string[] modList) {
            Console.WriteLine();
            Console.WriteLine("Listing files for mods ... ");
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (string modName in modList) {
                Console.Write("  ");
                WriteColored(ConsoleColor.DarkCyan, modName);
                Console.Write(" ... ");
                WriteColored(ConsoleColor.DarkYellow, 0);
                Console.Write(" files found.");

                string modPath = Path.Combine(_modOrganizerDataDir, "mods", modName);
                IEnumerable<string> files = Directory.EnumerateFiles(modPath, "*.*", SearchOption.AllDirectories);
                int counter = 0;
                foreach (string file in files) {
                    string relativePath = file.Substring(modPath.Length + 1);

                    // meta.ini is MO file ...
                    if (relativePath == "meta.ini") {
                        continue;
                    }

                    result[relativePath] = modName;
                    Console.Write("\r  ");
                    WriteColored(ConsoleColor.DarkCyan, modName);
                    Console.Write(" ... ");
                    WriteColored(ConsoleColor.DarkYellow, ++counter);
                    Console.Write(" files found.");
                }

                Console.WriteLine();
            }

            return result;
        }

        private static void UpdateLinks(Dictionary<string, string> diff) {
            Console.WriteLine();
            Console.Write("Updating file links ... ");
            int counter = 0;
            int prevLen = 0;
            foreach (KeyValuePair<string, string> item in diff) {
                int len = $"Updating file links ... {counter} of {diff.Count} ({item.Key}).".Length;
                Console.Write("\rUpdating file links ... ");
                WriteColored(ConsoleColor.DarkYellow, ++counter);
                Console.Write(" of ");
                WriteColored(ConsoleColor.DarkYellow, diff.Count);
                Console.Write(" (");
                string key = item.Key;
                if (item.Key.Length > 100) {
                    key = item.Key.Substring(0, 50) + "..." + item.Key.Substring(item.Key.Length - 45);
                }

                WriteColored(ConsoleColor.DarkCyan, key);
                Console.Write(").");
                if (prevLen > len) {
                    Console.Write(new string(' ', prevLen - len + 1));
                }

                prevLen = len;

                string destPath = Path.Combine(_modOrganizerConfig.GamePath, "Data", item.Key);
                if (File.Exists(destPath)) {
                    File.Delete(destPath);
                }

                if (item.Value != null) {
                    string srcPath = Path.Combine(_modOrganizerDataDir, "mods", item.Value, item.Key);
                    string dirPath = Path.GetDirectoryName(destPath);
                    Debug.Assert(dirPath != null, nameof(dirPath) + " != null");
                    if (!Directory.Exists(dirPath)) {
                        Directory.CreateDirectory(dirPath);
                    }

                    PInvoke.CreateHardLink(destPath, srcPath, IntPtr.Zero);
                }
            }

            Console.Write("Updating file links ... ");
            WriteColored(ConsoleColor.DarkCyan, "done");
            Console.Write(new string(' ', prevLen));
            Console.WriteLine();
        }

        private static void WriteColored(ConsoleColor color, object value) {
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ResetColor();
        }

        private static void WriteError(Exception ex) {
            Console.WriteLine();
            WriteColored(ConsoleColor.Red, "ERROR: " + ex.Message);
            Console.WriteLine();
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();
        }

        private static void WriteFooter() {
            WriteColored(ConsoleColor.Yellow, "Press any key to exit ...");
            Console.WriteLine();
            Console.ReadKey();
        }

        private static void WriteHeader() {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ModOrganizerHelper.Properties.LICENSE")) {
                Debug.Assert(stream != null, nameof(stream) + " != null");
                using (StreamReader reader = new StreamReader(stream)) {
                    Console.WriteLine(reader.ReadToEnd());
                }
            }

            Console.WriteLine(new string('-', 80));
            Console.WriteLine();
            Console.WriteLine("This tool will update links in Fallout4 data folder based on MO modlist ...");
            Console.WriteLine();
            WriteColored(ConsoleColor.Yellow, "Make sure NOT to run this tool from MO!");
            Console.WriteLine();
            Console.WriteLine("  (otherwise VFS will kick in and all links will be created in override folder)");
            Console.WriteLine();
            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }

        private static void WriteSuccess() {
            Console.WriteLine();
            Console.WriteLine();
            Console.Write("All links upated, now you can start game without running ");
            WriteColored(ConsoleColor.Yellow, "ModOrganizer");
            Console.WriteLine(".");
            Console.WriteLine();
        }
    }
}
