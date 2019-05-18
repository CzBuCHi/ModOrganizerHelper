using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace ModOrganizerHelper
{
    public class ModOrganizerConfig
    {
        // group -> key -> value
        private readonly Dictionary<string, Dictionary<string, string>> _data = new Dictionary<string, Dictionary<string, string>>();

        public ModOrganizerConfig(string initFilePath) {
            // really simple ini parser - no error handling
            string groupKey = null;
            Dictionary<string, string> group = null;
            foreach (string line in File.ReadLines(initFilePath)) {
                if (string.IsNullOrWhiteSpace(line)) {
                    continue;
                }

                if (line.TrimStart().StartsWith("#")) {
                    continue;
                }

                Match match = Regex.Match(line, @"^\[(?<group>.*?)\]$", RegexOptions.Compiled);
                if (match.Success) {
                    if (groupKey != null && group.Count > 0) {
                        _data.Add(groupKey, group);
                    }

                    group = new Dictionary<string, string>();
                    groupKey = match.Groups["group"].Value;
                    continue;
                }

                Debug.Assert(group != null, nameof(group) + " != null");
                int index = line.IndexOf('=');
                string key = line.Substring(0, index);
                string value = line.Substring(index + 1);
                group.Add(key, value);
            }
        }

        public string GamePath => _data["General"]["gamePath"];

        public string SelectedProfile => _data["General"]["selected_profile"];

        public string BaseDirectory => _data["Settings"]["base_directory"];

        public string ModDirectory => _data["Settings"]["mod_directory"].Replace("%BASE_DIR%", BaseDirectory);

        public string ProfilesDirectory => _data["Settings"]["profiles_directory"].Replace("%BASE_DIR%", BaseDirectory);
    }
}
