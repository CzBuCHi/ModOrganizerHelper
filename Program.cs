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
        private readonly ModOrganizerConfig _ModOrganizerConfig;

        private readonly string _ProfilePath;

        private readonly string _RootDir;

        private Program(string rootDir) {
            _RootDir = rootDir;
            _ModOrganizerConfig = new ModOrganizerConfig(Path.Combine(_RootDir, "ModOrganizer.ini"));
            _ProfilePath = Path.Combine(_RootDir, "profiles", _ModOrganizerConfig.SelectedProfile);

            Console.Write("Selected profile: ");
            DarkYellow(_ModOrganizerConfig.SelectedProfile);
            Console.WriteLine();
            Console.WriteLine();
            if (Settings.Default.ProfileName != _ModOrganizerConfig.SelectedProfile) {
                Console.WriteLine("Profile was changed from last run ...");
                Console.WriteLine();
                CopyIniFiles();
                LinkSaves();
                Settings.Default.ProfileName = _ModOrganizerConfig.SelectedProfile;
            }

            // list of all mods
            string[] modList = LoadModList();

            // file relative path -> mod name
            Dictionary<string, string> fileLinks = ResolveFileLinks(modList);

            // file relative path -> mod name -or- null when link is obsolete
            Dictionary<string, string> delta = GetFileLinksDelta(fileLinks);

            UpdateLinks(delta);

            Settings.Default.LinkList = string.Join("\r\n", fileLinks.Select(o => o.Key + "|" + o.Value));
            Settings.Default.Save();
        }

        private string DocumentsConfigPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Fallout4");

        public static void Main(string[] args) {
            try {
                if (args.Length == 0) {
                    Console.WriteLine("USAGE: ModOrganizerHelper.exe Path");
                    Console.WriteLine();
                    Console.WriteLine("  Path: path to mod organizer settings folder (usually at '%LOCALAPPDATA%\\ModOrganizer')");
                    Console.WriteLine();
                    return;
                }

                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ModOrganizerHelper.Properties.LICENSE")) {
                    Debug.Assert(stream != null, nameof(stream) + " != null");
                    using (StreamReader reader = new StreamReader(stream)) {
                        Console.WriteLine(reader.ReadToEnd());
                    }
                }

                Console.WriteLine(new string('-', 80));
                Console.WriteLine();
                Console.WriteLine("This tool will update links in game data folder based on MO modlist ...");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Make sure NOT to run this tool from MO!");
                Console.ResetColor();
                Console.WriteLine("  (otherwise VFS will kick in and all links will be created in override folder)");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Press any key to continue ...");
                Console.ReadKey();

                // ReSharper disable once ObjectCreationAsStatement
                new Program(args[0]);

                Console.WriteLine();
                Console.WriteLine();
                Console.Write("All links upated, now you can start game without running ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("ModOrganizer");
                Console.ResetColor();
                Console.WriteLine(".");
                Console.WriteLine();
            } catch (Exception ex) {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: " + ex.Message);
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine();
            } finally {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Press any key to exit ...");
                Console.ResetColor();
                Console.ReadKey();
            }
        }

        private static void DarkCyan(object value) {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(value);
            Console.ResetColor();
        }

        private static void DarkYellow(object value) {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(value);
            Console.ResetColor();
        }

        private void CopyIniFiles() {
            Console.Write("  Copying ini files ... ");
            string[] files = {
                "fallout4.ini",
                "Fallout4Custom.ini",
                "fallout4prefs.ini"
            };
            foreach (string file in files) {
                string filePath = Path.Combine(_ProfilePath, file);
                string destPath = Path.Combine(DocumentsConfigPath, file);
                if (File.Exists(destPath)) {
                    File.Delete(destPath);
                }

                if (File.Exists(filePath)) {
                    File.Copy(filePath, destPath);
                }
            }

            DarkCyan("done");
            Console.WriteLine();
        }

        private Dictionary<string, string> GetFileLinksDelta(Dictionary<string, string> fileLinks) {
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

            DarkYellow(actual);
            Console.Write(" up to date, ");
            DarkYellow(insert);
            Console.Write(" new, ");
            DarkYellow(update);
            Console.Write(" changed and ");
            DarkYellow(delete);
            Console.WriteLine(" deleted links found.");
            return result;
        }

        private void LinkSaves() {
            Console.Write("  Creating save folder link ... ");
            string srcDir = Path.Combine(Directory.GetCurrentDirectory(), "Saves", _ModOrganizerConfig.SelectedProfile);
            string destDir = Path.Combine(DocumentsConfigPath, "Saves");
            if (Directory.Exists(destDir)) {
                Directory.Delete(destDir);
            }

            PInvoke.CreateSymbolicLink(destDir, srcDir, PInvoke.SYMBOLIC_LINK_FLAG.Directory);
            DarkCyan("done");
            Console.WriteLine();
        }

        private Dictionary<string, string> LoadLinkList() {
            if (string.IsNullOrEmpty(Settings.Default.LinkList)) {
                return new Dictionary<string, string>();
            }

            return Settings.Default.LinkList
                           .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                           .Select(o => o.Split('|'))
                           .ToDictionary(o => o[0], o => o[1]);
        }

        private string[] LoadModList() {
            Console.WriteLine();
            Console.Write("Loading mod list ... ");
            string modList = Path.Combine(_ProfilePath, "modlist.txt");
            string[] result = File.ReadLines(modList, Encoding.UTF8)
                                  .Where(o => o[0] == '+')
                                  .Select(o => o.Substring(1))
                                  .Reverse()
                                  .ToArray();

            DarkYellow(modList.Length);
            Console.Write(" mods found out of which ");
            DarkYellow(result.Length);
            Console.WriteLine(" are active.");
            return result;
        }

        private Dictionary<string, string> ResolveFileLinks(string[] modList) {
            Console.WriteLine();
            Console.WriteLine("Listing files for mods ... ");
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (string modName in modList) {
                Console.Write("  ");
                DarkCyan(modName);
                Console.Write(" ... ");
                DarkYellow(0);
                Console.Write(" files found.");

                string modPath = Path.Combine(_RootDir, "mods", modName);
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
                    DarkCyan(modName);
                    Console.Write(" ... ");
                    DarkYellow(++counter);
                    Console.Write(" files found.");
                }

                Console.WriteLine();
            }

            return result;
        }

        private void UpdateLinks(Dictionary<string, string> delta) {
            Console.WriteLine();
            Console.Write("Updating file links ... ");
            int counter = 0;
            int prevLen = 0;
            foreach (KeyValuePair<string, string> item in delta) {
                int len = $"Updating file links ... {counter} of {delta.Count} ({item.Key}).".Length;
                Console.Write("\rUpdating file links ... ");
                DarkYellow(++counter);
                Console.Write(" of ");
                DarkYellow(delta.Count);
                Console.Write(" (");
                string key = item.Key;
                if (item.Key.Length > 100) {
                    key = item.Key.Substring(0, 50) + "..." + item.Key.Substring(item.Key.Length - 45);
                }

                DarkCyan(key);
                Console.Write(").");
                if (prevLen > len) {
                    Console.Write(new string(' ', prevLen - len + 1));
                }

                prevLen = len;

                string destPath = Path.Combine(_ModOrganizerConfig.GamePath, "Data", item.Key);
                if (File.Exists(destPath)) {
                    File.Delete(destPath);
                }

                if (item.Value != null) {
                    string srcPath = Path.Combine(_RootDir, "mods", item.Value, item.Key);
                    string dirPath = Path.GetDirectoryName(srcPath);
                    Debug.Assert(dirPath != null, nameof(dirPath) + " != null");
                    if (!Directory.Exists(dirPath)) {
                        Directory.CreateDirectory(dirPath);
                    }

                    PInvoke.CreateSymbolicLink(destPath, srcPath, PInvoke.SYMBOLIC_LINK_FLAG.File);
                }
            }

            Console.WriteLine();
        }
    }
}
