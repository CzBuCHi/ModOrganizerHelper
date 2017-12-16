using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ModOrganizerHelper
{
    /// <summary>
    /// Primitive INI file reader
    /// </summary>
    public class IniFile
    {
        /// <summary>
        /// Group -> Key -> Value
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, string>> _Ini = new Dictionary<string, Dictionary<string, string>>();

        public IniFile(string initFilePath) {
            string groupKey = "Empty";
            Dictionary<string, string> group = new Dictionary<string, string>();
            foreach (string line in File.ReadLines(initFilePath)) {
                // empty line
                if (string.IsNullOrWhiteSpace(line)) {
                    continue;
                }

                // comment
                if (line.TrimStart().StartsWith("#")) {
                    continue;
                }

                // group start?
                Match match = Regex.Match(line, @"^\[(?<group>.*?)\]$", RegexOptions.Compiled);
                if (match.Success) {
                    if (group.Count > 0) {
                        _Ini.Add(groupKey, group);
                    }

                    group = new Dictionary<string, string>();
                    groupKey = match.Groups["group"].Value;
                    continue;
                }

                // '=' in key not supported
                // key or value in quotes not supported
                int index = line.IndexOf('=');
                string key = line.Substring(0, index);
                string value = line.Substring(index + 1);
                group.Add(key, value);
            }
        }

        public string GetValue(string group, string key) {
            return _Ini[group][key];
        }
    }
}
