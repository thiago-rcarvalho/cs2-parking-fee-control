using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Colossal.IO.AssetDatabase;
using Unity.Entities;

namespace ParkingFeeControl
{
    /// <summary>
    /// Configuration class for Parking Fee Control mod.
    /// Only contains fee settings that are persisted to JSON.
    /// </summary>
    public class ParkingFeeConfig
    {
        public const string DistrictsCategoryType = "districts";
        private const string DistrictKeyPrefix = "district";
        public class PrefabEntry
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            // Optional override fee for this prefab
            [JsonProperty("fee")]
            public int? Fee { get; set; }
        }

        public class Category
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("icon")]
            public string Icon { get; set; } = string.Empty;

            [JsonProperty("defaultFee")]
            public int DefaultFee { get; set; }

            [JsonProperty("prefabs")]
            public List<PrefabEntry> Prefabs { get; set; } = new List<PrefabEntry>();
        }

        [JsonProperty("categories")]
        public List<Category> Categories { get; set; } = new List<Category>();

        /// <summary>
        /// Create a new ParkingFeeConfig with default categories.
        /// </summary>
        public static ParkingFeeConfig CreateDefault()
        {
            return new ParkingFeeConfig
            {
                Categories = new List<Category>
                {
                    new Category
                    {
                        Type = "car",
                        DefaultFee = 10,
                        Prefabs = new List<PrefabEntry>
                        {
                            new PrefabEntry { Name = "AutomatedParkingBuilding01" },
                            new PrefabEntry { Name = "ParkingHall01" },
                            new PrefabEntry { Name = "ParkingHall02" },
                            new PrefabEntry { Name = "ParkingHall03" },
                            new PrefabEntry { Name = "ParkingHall04" },
                            new PrefabEntry { Name = "ParkingLot01" },
                            new PrefabEntry { Name = "ParkingLot02" },
                            new PrefabEntry { Name = "ParkingLot03" },
                            new PrefabEntry { Name = "ParkingLot04" },
                            new PrefabEntry { Name = "ParkingLot06" },
                            new PrefabEntry { Name = "ParkingLot07" },
                            new PrefabEntry { Name = "ParkingLot08" },
                            new PrefabEntry { Name = "ParkingLot09" },
                            new PrefabEntry { Name = "ParkingLot10" },
                            new PrefabEntry { Name = "ParkingLot11" },
                            new PrefabEntry { Name = "ParkingLot12" },
                            new PrefabEntry { Name = "ParkingLot13" },
                            new PrefabEntry { Name = "ParkingLot14" },
                            new PrefabEntry { Name = "ParkingLot15" },
                            new PrefabEntry { Name = "ParkingLot16" },
                            new PrefabEntry { Name = "ParkingLot17" }
                        }
                    },
                    new Category
                    {
                        Type = "bicycle",
                        DefaultFee = 0,
                        Prefabs = new List<PrefabEntry>
                        {
                            new PrefabEntry { Name = "BicycleParkingArea01" },
                            new PrefabEntry { Name = "BicycleParkingArea02" },
                            new PrefabEntry { Name = "BicycleParkingArea03" },
                            new PrefabEntry { Name = "BicycleParkingHall01" },
                            new PrefabEntry { Name = "BicycleParkingHall02" },
                            new PrefabEntry { Name = "BicycleParkingHall03" },
                            new PrefabEntry { Name = "BicycleStorage01" },
                            new PrefabEntry { Name = "BicycleStorage02" },
                            new PrefabEntry { Name = "BicycleStorage03" }
                        }
                    },
                    new Category
                    {
                        Type = "motorcycle",
                        DefaultFee = 10,
                        Prefabs = new List<PrefabEntry>
                        {
                            new PrefabEntry { Name = "ParkingLot05" }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Load configuration from JSON file.
        /// </summary>
        /// <param name="showLog">Force showing filter log even if DebugLogging is disabled</param>
        public static ParkingFeeConfig Load(bool showLog = false)
        {
            try
            {
                string configPath = System.IO.Path.Combine(Mod.ModPath, "parking-config.json");
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var config = JsonConvert.DeserializeObject<ParkingFeeConfig>(json);
                    if (config != null)
                    {
                        // Merge with shipping/embedded parking-data so updates can add new prefabs/categories
                        var parkingData = ParkingDataLoader.Load(Mod.ModPath, showLog);
                        if (parkingData != null)
                        {
                            var changed = MergeWithParkingData(config, parkingData);
                            if (changed)
                            {
                                config.Save();
                                ModLogger.Debug("parking-config.json updated with new entries from parking-data.json");
                            }
                        }

                        ModLogger.Debug("Configuration loaded from JSON file");

                        return config;
                    }
                }
                else
                {
                    ModLogger.Debug("Configuration file not found, creating default from parking-data.json if available");

                    // Try to create a default config from parking-data.json (allows adding supported prefabs without code changes)
                    var parkingData = ParkingDataLoader.Load(Mod.ModPath, showLog);
                    ParkingFeeConfig defaultConfig;
                    if (parkingData != null && parkingData.Categories.Count > 0)
                    {
                        defaultConfig = new ParkingFeeConfig();
                        foreach (var cat in parkingData.Categories)
                        {
                            var newCat = new Category
                            {
                                Type = cat.Type,
                                DefaultFee = cat.DefaultFee,
                                Icon = cat.Icon,
                                Prefabs = new List<PrefabEntry>()
                            };

                            foreach (var prefab in cat.Prefabs)
                            {
                                newCat.Prefabs.Add(new PrefabEntry { Name = prefab.Name });
                            }

                            defaultConfig.Categories.Add(newCat);
                        }
                        defaultConfig.Save();
                        ModLogger.Debug("Created default configuration from parking-data.json");
                    }
                    else
                    {
                        defaultConfig = CreateDefault();
                        defaultConfig.Save();
                        ModLogger.Debug("Created default configuration from embedded defaults");
                    }

                    return defaultConfig;
                }
            }
            catch (Exception ex)
            {
                ModLogger.Error($"Failed to load configuration: {ex.Message}");
            }

            return CreateDefault();
        }

        /// <summary>
        /// Save configuration to JSON file.
        /// </summary>
        public void Save()
        {
            try
            {
                string configPath = System.IO.Path.Combine(Mod.ModPath, "parking-config.json");
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                ModLogger.Error($"Failed to save configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Log current configuration settings.
        /// </summary>
        public void LogSettings()
        {
            if (Mod.Settings?.DebugLogging != true)
                return;

            ModLogger.Debug("Configuration settings:");
            LogGeneralSettings();
            LogFeeSettings();
        }

        private void LogGeneralSettings()
        {
            try
            {
                var settings = Mod.Settings;
                if (settings != null)
                {
                    ModLogger.Debug($"  - Enabled: {settings.Enabled}");
                    ModLogger.Debug($"  - DebugLogging: {settings.DebugLogging}");
                    ModLogger.Debug($"  - UpdateFrequency: {settings.UpdateFrequencyMinutes} ({settings.GetUpdateFrequencySeconds()}s)");
                    ModLogger.Debug($"  - IgnoreTag: {settings.GetIgnoreTagString()}");
                }
                else
                {
                    ModLogger.Debug("  - General settings: <not initialized>");
                }
            }
            catch (Exception ex)
            {
                ModLogger.Warn($"Failed to log general settings: {ex.Message}");
            }
        }

        private void LogFeeSettings()
        {
            ModLogger.Debug("  - Categories:");
            foreach (var cat in Categories)
            {
                try
                {
                    ModLogger.Debug($"    - {cat.Type}: default ${cat.DefaultFee}, prefabs: {string.Join(", ", cat.Prefabs.Select(p => p.Name))}");
                }
                catch (Exception ex)
                {
                    ModLogger.Warn($"Failed to log category '{cat?.Type ?? "<null>"}': {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Get the parking fee for a specific prefab name. If a prefab-specific fee exists, return it; otherwise use the category default.
        /// </summary>
        public int GetParkingFeeForPrefab(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
                return 10; // fallback default

            foreach (var cat in Categories)
            {
                // Look for prefab entry
                var match = cat.Prefabs.FirstOrDefault(p => string.Equals(p.Name, prefabName, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    if (match.Fee.HasValue)
                        return match.Fee.Value;
                    return cat.DefaultFee;
                }
            }

            // No category matched - return fallback default
            return 10;
        }

        /// <summary>
        /// Get the parking fee for a district by its key. Falls back to category default.
        /// </summary>
        public int GetParkingFeeForDistrictKey(string districtKey)
        {
            if (string.IsNullOrWhiteSpace(districtKey))
                return 0;

            var category = Categories.FirstOrDefault(c => string.Equals(c.Type, DistrictsCategoryType, StringComparison.OrdinalIgnoreCase));
            if (category == null)
                return 0;

            var match = category.Prefabs.FirstOrDefault(p => string.Equals(p.Name, districtKey, StringComparison.OrdinalIgnoreCase));
            if (match != null && match.Fee.HasValue)
                return match.Fee.Value;

            return category.DefaultFee;
        }

        /// <summary>
        /// Build a stable key for a district entity.
        /// </summary>
        public static string GetDistrictKey(string districtName)
        {
            if (string.IsNullOrWhiteSpace(districtName))
            {
                return $"{DistrictKeyPrefix}:unknown";
            }

            return $"{DistrictKeyPrefix}:{districtName.Trim()}";
        }

        /// <summary>
        /// Create a deep clone of the configuration for UI binding.
        /// </summary>
        private static bool MergeWithParkingData(ParkingFeeConfig config, ParkingDataLoader.ParkingData parkingData)
        {
            bool changed = false;

            // Build lookup for incoming data (case-insensitive)
            var dataCatsByType = parkingData.Categories
                .ToDictionary(dc => dc.Type, StringComparer.OrdinalIgnoreCase);

            // Remove categories that no longer exist in parking-data
            var toRemoveCats = config.Categories
                .Where(c => !dataCatsByType.ContainsKey(c.Type))
                .ToList();
            foreach (var rem in toRemoveCats)
            {
                config.Categories.Remove(rem);
                changed = true;
            }

            // Sync existing categories and add new ones
            foreach (var dataCat in parkingData.Categories)
            {
                var existingCat = config.Categories.FirstOrDefault(c => string.Equals(c.Type, dataCat.Type, StringComparison.OrdinalIgnoreCase));
                if (existingCat == null)
                {
                    // New category -> add with prefabs (no fees)
                    var newCat = new Category
                    {
                        Type = dataCat.Type,
                        DefaultFee = dataCat.DefaultFee,
                        Icon = dataCat.Icon,
                        Prefabs = dataCat.Prefabs.Select(p => new PrefabEntry { Name = p.Name }).ToList()
                    };
                    config.Categories.Add(newCat);
                    changed = true;
                    continue;
                }

                // Do not override user-configured default fee for existing categories

                if (!string.Equals(existingCat.Icon, dataCat.Icon, StringComparison.OrdinalIgnoreCase))
                {
                    existingCat.Icon = dataCat.Icon ?? string.Empty;
                    changed = true;
                }

                if (!string.Equals(existingCat.Type, DistrictsCategoryType, StringComparison.OrdinalIgnoreCase))
                {
                    // Build set of allowed prefabs for this category (already filtered by installed mods)
                    var allowedPrefabs = new HashSet<string>(dataCat.PrefabNames, StringComparer.OrdinalIgnoreCase);

                    // Remove prefabs that authors removed from parking-data (even if user changed fee)
                    var prefabsToRemove = existingCat.Prefabs.Where(p => !allowedPrefabs.Contains(p.Name)).ToList();
                    foreach (var rem in prefabsToRemove)
                    {
                        existingCat.Prefabs.Remove(rem);
                        changed = true;
                    }

                    // Add new prefabs present in parking-data but missing in config
                    foreach (var prefab in dataCat.Prefabs)
                    {
                        if (!existingCat.Prefabs.Any(p => string.Equals(p.Name, prefab.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            existingCat.Prefabs.Add(new PrefabEntry { Name = prefab.Name });
                            changed = true;
                        }
                    }
                }
            }

            return changed;
        }

    }
}
