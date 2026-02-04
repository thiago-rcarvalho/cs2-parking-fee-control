using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Colossal.IO.AssetDatabase;
using Colossal.Json;
using Game.SceneFlow;
using Newtonsoft.Json;

namespace ParkingFeeControl
{
    /// <summary>
    /// Loads parking-data.json which enumerates supported prefabs per category.
    /// This is intended to be a simple, user-editable list of supported prefabs
    /// so maintainers can add new prefabs without touching the C# code.
    /// </summary>
    public static class ParkingDataLoader
    {
        /// <summary>
        /// Represents a prefab entry with optional mod dependency.
        /// ModId should be the Paradox Mods platformID (numeric ID as string).
        /// </summary>
        public class PrefabEntry
        {
            [JsonProperty("name")]
            public string Name { get; set; } = string.Empty;

            [JsonProperty("modId")]
            public string? ModId { get; set; } = null;

            public PrefabEntry() { }

            public PrefabEntry(string name, string? modId = null)
            {
                Name = name;
                ModId = modId;
            }
        }

        public class DataCategory
        {
            [JsonProperty("type")]
            public string Type { get; set; } = string.Empty;

            [JsonProperty("defaultFee")]
            public int DefaultFee { get; set; }

            [JsonProperty("icon")]
            public string Icon { get; set; } = string.Empty;

            [JsonProperty("prefabs")]
            public List<PrefabEntry> Prefabs { get; set; } = new List<PrefabEntry>();

            /// <summary>
            /// Gets list of prefab names (filtered by installed mods).
            /// </summary>
            [JsonIgnore]
            public List<string> PrefabNames => Prefabs.Select(p => p.Name).ToList();
        }

        public class ParkingData
        {
            [JsonProperty("categories")]
            public List<DataCategory> Categories { get; set; } = new List<DataCategory>();
        }

        /// <summary>
        /// Gets the set of installed mods using Paradox Mods platformID.
        /// All mods (code and asset mods) are detected via AssetDatabase.
        /// </summary>
        /// <returns>HashSet of platformIDs (as strings) for installed mods</returns>
        private static HashSet<string> GetInstalledMods()
        {
            var installedModIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                // Detect all mods via AssetDatabase
                // This includes both code mods and asset-only mods from Paradox Mods
                var assetDatabase = AssetDatabase.global;
                if (assetDatabase != null)
                {
                    var prefabAssets = assetDatabase.GetAssets<PrefabAsset>(new SearchFilter<PrefabAsset>());
                    
                    foreach (var asset in prefabAssets)
                    {
                        try
                        {
                            var meta = asset.GetMeta();
                            var platformId = meta.platformID;
                            
                            // Only add if platformID is valid (>0)
                            // platformID = 0 means vanilla assets or local/dev mods
                            if (platformId > 0)
                            {
                                installedModIds.Add(platformId.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            // Skip this asset on error
                            ModLogger.Warn($"Failed to get platformID for asset: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModLogger.Error($"Failed to detect installed mods: {ex}");
            }

            return installedModIds;
        }

        /// <summary>
        /// Filters prefabs based on installed mods. O(n + m) complexity where n=mods, m=prefabs.
        /// </summary>
        /// <param name="showLog">Force showing filter log even if DebugLogging is disabled</param>
        private static void FilterPrefabsByInstalledMods(ParkingData data, bool showLog = false)
        {
            var installedMods = GetInstalledMods();
            int totalPrefabs = 0;
            int filteredPrefabs = 0;

            foreach (var category in data.Categories)
            {
                totalPrefabs += category.Prefabs.Count;

                // Filter out prefabs that require mods that aren't installed
                category.Prefabs = category.Prefabs.Where(prefab =>
                {
                    // Vanilla prefabs (modId == null) are always included
                    if (string.IsNullOrEmpty(prefab.ModId))
                        return true;

                    // Check if required mod is installed (O(1) lookup)
                    bool isInstalled = installedMods.Contains(prefab.ModId);

                    if (!isInstalled)
                    {
                        ModLogger.Debug($"Filtering out prefab '{prefab.Name}' - required mod '{prefab.ModId}' not installed");
                    }

                    return isInstalled;
                }).ToList();

                filteredPrefabs += category.Prefabs.Count;
            }

            if (showLog)
            {
                ModLogger.Info($"Prefab filtering: {filteredPrefabs}/{totalPrefabs} prefabs available after mod checks");
            }
            else
            {
                ModLogger.Debug($"Prefab filtering: {filteredPrefabs}/{totalPrefabs} prefabs available after mod checks");
            }
        }

        public static ParkingData? Load(string modPath, bool showLog = false)
        {
            try
            {
                // Prefer an external parking-data.json placed next to the mod (useful during development),
                // otherwise fall back to the embedded resource inside the DLL so releases don't require the file.
                if (!string.IsNullOrEmpty(modPath))
                {
                    var path = Path.Combine(modPath, "parking-data.json");
                    if (File.Exists(path))
                    {
                        var json = File.ReadAllText(path);
                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            var data = JsonConvert.DeserializeObject<ParkingData>(json);
                            if (data != null)
                            {
                                FilterPrefabsByInstalledMods(data, showLog);
                                return data;
                            }
                        }
                    }
                }

                // Try embedded resource: ParkingFeeControl.parking-data.json
                var asm = typeof(ParkingDataLoader).Assembly;
                var resourceName = asm.GetName().Name + ".parking-data.json";
                using (var stream = asm.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var json = reader.ReadToEnd();
                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                var data = JsonConvert.DeserializeObject<ParkingData>(json);
                                if (data != null)
                                {
                                    FilterPrefabsByInstalledMods(data, showLog);
                                    return data;
                                }
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                ModLogger.Warn($"Failed to load parking-data.json: {ex.Message}");
                return null;
            }
        }
    }
}
