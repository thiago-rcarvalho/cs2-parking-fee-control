using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Colossal.Json;
using Game;
using Colossal;
using ParkingFeeControl;

namespace ParkingFeeControl
{

    /// <summary>
    /// Loads external JSON localization files from the mod's `Locale` folder
    /// and registers them with the game's localization manager as IDictionarySource.
    /// This mirrors the approach used by reference mods (FindIt) but reads files
    /// from disk so translators can edit plain JSON files.
    /// </summary>
    public static class LocaleFileLoader
    {
        /// <summary>
        /// Loads all JSON files in the `Locale` folder and returns a mapping of
        /// localeId -> dictionary. Does not register them with the localization
        /// manager (registration should happen from Mod.cs where GameManager is
        /// reliably available at compile time).
        /// </summary>
        public static Dictionary<string, Dictionary<string, string>> LoadDictionaries(string modPath)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();

            if (string.IsNullOrEmpty(modPath))
                return result;

            var localeDir = Path.Combine(modPath, "Locale");
            if (!Directory.Exists(localeDir))
                return result;

            foreach (var file in Directory.GetFiles(localeDir, "*.json", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var localeId = fileName; // expect filenames like `en-US.json`, `zh-HANS.json`

                    string json;
                    using (var reader = new StreamReader(file, Encoding.UTF8))
                    {
                        json = reader.ReadToEnd();
                    }

                    if (string.IsNullOrWhiteSpace(json))
                        continue;

                    var loaded = JSON.Load(json);
                    JSON.MakeInto<Dictionary<string, string>>(loaded, out var dictionary);
                    if (dictionary == null)
                        continue;

                    result[localeId] = dictionary;
                }
                catch (Exception ex)
                {
                    // Avoid crashing the mod load if a single file fails
                    ModLogger.Warn($"Failed to load locale file '{file}': {ex.Message}");
                }
            }

            return result;
        }

        public class FileDictionarySource : IDictionarySource
        {
            private readonly Dictionary<string, string> _dictionary;
            public FileDictionarySource(Dictionary<string, string> dictionary)
            {
                _dictionary = dictionary;
            }

            public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
            {
                return _dictionary;
            }

            public void Unload() { }
        }
    }
}
