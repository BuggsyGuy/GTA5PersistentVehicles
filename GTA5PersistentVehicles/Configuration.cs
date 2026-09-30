using System;
using System.IO;
using System.Windows.Forms;

namespace GTA5PersistentVehicles
{
    /// <summary>Reads the user-editable Settings.ini file.</summary>
    internal sealed class Configuration
    {
        public bool Enabled { get; private set; } = true;
        public int MaxSavedVehicles { get; private set; } = 10;
        public Keys ClearVehiclesKey { get; private set; } = Keys.K;

        public static Configuration Load(string path)
        {
            Configuration result = new Configuration();
            IniFile ini = IniFile.Load(path);
            result.Enabled = ini.GetBoolean("Main", "Enabled", true);
            result.MaxSavedVehicles = Math.Max(1, ini.GetInt("Main", "MaxSavedVehicles", 10));

            Keys key;
            if (Enum.TryParse(ini.GetString("Main", "ClearVehiclesKey", "K"), true, out key))
                result.ClearVehiclesKey = key;

            return result;
        }
    }

    internal sealed class IniFile
    {
        private readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>> _sections =
            new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        public static IniFile Load(string path)
        {
            IniFile file = new IniFile();
            if (!File.Exists(path))
                return file;

            string section = string.Empty;
            foreach (string raw in File.ReadAllLines(path))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#"))
                    continue;
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    section = line.Substring(1, line.Length - 2).Trim();
                    if (!file._sections.ContainsKey(section))
                        file._sections[section] = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    continue;
                }
                int equals = line.IndexOf('=');
                if (equals <= 0 || section.Length == 0)
                    continue;
                file._sections[section][line.Substring(0, equals).Trim()] = line.Substring(equals + 1).Trim();
            }
            return file;
        }

        public string GetString(string section, string key, string fallback)
        {
            System.Collections.Generic.Dictionary<string, string> values;
            string value;
            return _sections.TryGetValue(section, out values) && values.TryGetValue(key, out value) ? value : fallback;
        }

        public int GetInt(string section, string key, int fallback)
        {
            int value;
            return int.TryParse(GetString(section, key, fallback.ToString()), out value) ? value : fallback;
        }

        public bool GetBoolean(string section, string key, bool fallback)
        {
            bool value;
            return bool.TryParse(GetString(section, key, fallback.ToString()), out value) ? value : fallback;
        }
    }
}
