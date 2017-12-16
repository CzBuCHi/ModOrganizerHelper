namespace ModOrganizerHelper
{
    public class ModOrganizerConfig : IniFile
    {
        public ModOrganizerConfig(string initFilePath)
            : base(initFilePath) {
        }

        public string SelectedProfile => GetValue("General", "selected_profile");
        
        public string GamePath => GetValue("General", "gamePath");
    }
}