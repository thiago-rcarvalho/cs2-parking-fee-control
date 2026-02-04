using Colossal.UI.Binding;
using Game.UI;
using Game.UI.InGame;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using Game.Prefabs;
using Game.SceneFlow;

namespace ParkingFeeControl.UI
{
    public class ParkingFeeUIData : IJsonWritable
    {
        public List<CategoryData> categories { get; set; } = new List<CategoryData>();

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(GetType().FullName);
            writer.PropertyName("categories");
            writer.ArrayBegin(categories.Count);
            foreach (var category in categories)
            {
                category.Write(writer);
            }
            writer.ArrayEnd();
            writer.TypeEnd();
        }

        public class CategoryData : IJsonWritable
        {
            public string type { get; set; } = string.Empty;
            public string icon { get; set; } = string.Empty;
            public float defaultFee { get; set; }
            public List<PrefabData> prefabs { get; set; } = new List<PrefabData>();

            public void Write(IJsonWriter writer)
            {
                writer.TypeBegin(GetType().FullName);
                writer.PropertyName("type");
                writer.Write(type);
                writer.PropertyName("icon");
                writer.Write(icon);
                writer.PropertyName("defaultFee");
                writer.Write(defaultFee);
                writer.PropertyName("prefabs");
                writer.ArrayBegin(prefabs.Count);
                foreach (var prefab in prefabs)
                {
                    prefab.Write(writer);
                }
                writer.ArrayEnd();
                writer.TypeEnd();
            }
        }

        public class PrefabData : IJsonWritable
        {
            public string name { get; set; } = string.Empty;
            public string displayName { get; set; } = string.Empty;
            public string thumbnail { get; set; } = string.Empty;
            public float fee { get; set; }

            public void Write(IJsonWriter writer)
            {
                writer.TypeBegin(GetType().FullName);
                writer.PropertyName("name");
                writer.Write(name);
                writer.PropertyName("displayName");
                writer.Write(displayName);
                writer.PropertyName("thumbnail");
                writer.Write(thumbnail);
                writer.PropertyName("fee");
                writer.Write(fee);
                writer.TypeEnd();
            }
        }
    }

    public struct CategoryFeeUpdate : IJsonReadable
    {
        public string categoryType;
        public float newFee;

        public void Read(IJsonReader reader)
        {
            reader.ReadMapBegin();
            reader.ReadProperty("categoryType");
            reader.Read(out categoryType);
            reader.ReadProperty("newFee");
            reader.Read(out newFee);
            reader.ReadMapEnd();
        }
    }

    public struct PrefabFeeUpdate : IJsonReadable
    {
        public string categoryType;
        public string prefabName;
        public float newFee;

        public void Read(IJsonReader reader)
        {
            reader.ReadMapBegin();
            reader.ReadProperty("categoryType");
            reader.Read(out categoryType);
            reader.ReadProperty("prefabName");
            reader.Read(out prefabName);
            reader.ReadProperty("newFee");
            reader.Read(out newFee);
            reader.ReadMapEnd();
        }
    }

    public partial class ParkingFeeUISystem : UISystemBase
    {
        private ValueBinding<ParkingFeeUIData> _configBinding;
        private TriggerBinding<CategoryFeeUpdate> _updateCategoryFeeTrigger;
        private TriggerBinding<PrefabFeeUpdate> _updatePrefabFeeTrigger;
        private TriggerBinding _applyNowTrigger;
        private TriggerBinding _refreshConfigTrigger;

        private ParkingFeeUIData _currentConfig;
        private PrefabSystem _prefabSystem;
        private PrefabUISystem _prefabUISystem;
        private ImageSystem _imageSystem;
        private ParkingPolicyModifierSystem _policySystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            // Get required systems
            _prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            _prefabUISystem = World.GetOrCreateSystemManaged<PrefabUISystem>();
            _imageSystem = World.GetOrCreateSystemManaged<ImageSystem>();
            _policySystem = World.GetOrCreateSystemManaged<ParkingPolicyModifierSystem>();

            LoadConfigFromMod();

            AddBinding(_configBinding = new ValueBinding<ParkingFeeUIData>(
                "parkingfee",
                "config",
                _currentConfig
            ));

            AddBinding(_updateCategoryFeeTrigger = new TriggerBinding<CategoryFeeUpdate>(
                "parkingfee",
                "updateCategoryFee",
                UpdateCategoryFee
            ));

            AddBinding(_updatePrefabFeeTrigger = new TriggerBinding<PrefabFeeUpdate>(
                "parkingfee",
                "updatePrefabFee",
                UpdatePrefabFee
            ));

            AddBinding(_applyNowTrigger = new TriggerBinding(
                "parkingfee",
                "applyNow",
                ApplyNow
            ));

            AddBinding(_refreshConfigTrigger = new TriggerBinding(
                "parkingfee",
                "refreshConfig",
                RefreshConfigFromMod
            ));

            ModLogger.Info("[ParkingFeeControl] UI System initialized");
        }

        private void LoadConfigFromMod()
        {
            // Initialize _currentConfig with data from Mod.Config
            // This ensures the binding has a valid object to work with
            var config = Mod.Config;
            _currentConfig = new ParkingFeeUIData
            {
                categories = config.Categories.Select(c => {
                    // Build prefab UI data then sort by localized display name for a better UX
                    var prefabList = c.Prefabs.Select(p => new ParkingFeeUIData.PrefabData
                    {
                        name = p.Name,
                        displayName = GetDisplayName(p.Name),
                        thumbnail = GetThumbnail(p.Name),
                        fee = p.Fee ?? c.DefaultFee
                    }).ToList();

                    var sorted = prefabList.OrderBy(p => p.displayName ?? p.name, StringComparer.OrdinalIgnoreCase).ToList();

                    return new ParkingFeeUIData.CategoryData
                    {
                        type = c.Type,
                        icon = c.Icon,
                        defaultFee = c.DefaultFee,
                        prefabs = sorted
                    };
                }).ToList()
            };
        }

        private void RefreshConfigFromMod()
        {
            try
            {
                Mod.ReloadConfig();
                LoadConfigFromMod();
                NotifyConfigChanged();

                ModLogger.Debug("[ParkingFeeControl] UI config refreshed on panel open");
            }
            catch (Exception ex)
            {
                ModLogger.Warn($"[ParkingFeeControl] Failed to refresh UI config: {ex.Message}");
            }
        }

        private void ApplyNow()
        {
            try
            {
                _policySystem?.ApplyNow(resetTimer: true);
                ModLogger.Debug("[ParkingFeeControl] Apply now requested");
            }
            catch (Exception ex)
            {
                ModLogger.Warn($"[ParkingFeeControl] Failed to apply fees immediately: {ex.Message}");
            }
        }

        private void NotifyConfigChanged()
        {
            _currentConfig = CloneConfig(_currentConfig);
            _configBinding.Update(_currentConfig);
        }

        private static ParkingFeeUIData CloneConfig(ParkingFeeUIData source)
        {
            return new ParkingFeeUIData
            {
                categories = source.categories.Select(c => new ParkingFeeUIData.CategoryData
                {
                    type = c.type,
                    icon = c.icon,
                    defaultFee = c.defaultFee,
                    prefabs = c.prefabs.Select(p => new ParkingFeeUIData.PrefabData
                    {
                        name = p.name,
                        displayName = p.displayName,
                        thumbnail = p.thumbnail,
                        fee = p.fee
                    }).ToList()
                }).ToList()
            };
        }

        private void UpdateCategoryFee(CategoryFeeUpdate update)
        {
            var category = _currentConfig.categories.FirstOrDefault(c => c.type == update.categoryType);
            if (category != null)
            {
                float oldDefaultFee = category.defaultFee;

                // Update category default fee
                category.defaultFee = (float)Math.Round(update.newFee);

                // Update all prefabs in this category maintaining the difference from category
                foreach (var prefab in category.prefabs)
                {
                    float difference = oldDefaultFee - prefab.fee;
                    float newFee = update.newFee - difference;
                    // Clamp between 0 and 50
                    newFee = Math.Max(0, Math.Min(50, newFee));
                    prefab.fee = (float)Math.Round(newFee);
                }

                NotifyConfigChanged();
                
                var modCategory = Mod.Config.Categories.FirstOrDefault(c => c.Type == update.categoryType);
                if (modCategory != null)
                {
                    modCategory.DefaultFee = (int)Math.Round(update.newFee);

                    // Update prefabs in mod config maintaining the difference
                    foreach (var modPrefab in modCategory.Prefabs)
                    {
                        if (modPrefab.Fee.HasValue)
                        {
                            float difference = oldDefaultFee - modPrefab.Fee.Value;
                            float newFee = update.newFee - difference;
                            // Clamp between 0 and 50
                            newFee = Math.Max(0, Math.Min(50, newFee));
                            modPrefab.Fee = (int)Math.Round(newFee);
                        }
                    }
                }

                // Save config after changes
                Mod.Config.Save();

                // ModLogger.Info($"[ParkingFeeControl] Updated {update.categoryType} default fee to ${(int)Math.Round(update.newFee)}");
            }
        }

        private void UpdatePrefabFee(PrefabFeeUpdate update)
        {
            var category = _currentConfig.categories.FirstOrDefault(c => c.type == update.categoryType);
            if (category != null)
            {
                var prefab = category.prefabs.FirstOrDefault(p => p.name == update.prefabName);
                if (prefab != null)
                {
                    prefab.fee = (float)Math.Round(update.newFee);

                    NotifyConfigChanged();

                    var modCategory = Mod.Config.Categories.FirstOrDefault(c => c.Type == update.categoryType);
                    if (modCategory != null)
                    {
                        var modPrefab = modCategory.Prefabs.FirstOrDefault(p => p.Name == update.prefabName);
                        if (modPrefab != null)
                        {
                            modPrefab.Fee = (int)Math.Round(update.newFee);
                        }
                    }

                    // Save config after changes
                    Mod.Config.Save();

                    // ModLogger.Info($"[ParkingFeeControl] Updated {update.categoryType}/{update.prefabName} fee to ${update.newFee}");
                }
            }
        }

        // SaveConfig method intentionally removed; saving is handled via Mod.Config.Save()

        protected override void OnUpdate()
        {
            // UI System doesn't need constant updates
        }

        /// <summary>
        /// Gets the localized display name for a prefab, similar to FindIt-CSII mod.
        /// Tries different prefab types and uses the game's localization system.
        /// </summary>
        /// <param name="prefabName">The internal prefab name</param>
        /// <returns>The localized display name, or the original name if localization fails</returns>
        private string GetDisplayName(string prefabName)
        {
            try
            {
                string[] prefabTypes = { "BuildingPrefab", "Prefab" };
                
                foreach (var prefabType in prefabTypes)
                {
                    var prefabId = new PrefabID(prefabType, prefabName);
                    if (_prefabSystem.TryGetPrefab(prefabId, out var prefabBase))
                    {
                        // Use the same method as FindIt-CSII: GetTitleAndDescription from PrefabUISystem
                        _prefabUISystem.GetTitleAndDescription(_prefabSystem.GetEntity(prefabBase), out var titleId, out var _);

                        if (GameManager.instance.localizationManager.activeDictionary.TryGetValue(titleId, out var name))
                        {
                            return name;
                        }

                        break;
                    }
                }
                
                return prefabName;
            }
            catch
            {
                return prefabName;
            }
        }

        /// <summary>
        /// Gets the thumbnail image path for a prefab using the game's ImageSystem.
        /// Similar to how FindIt-CSII displays specific prefab icons.
        /// </summary>
        /// <param name="prefabName">The internal prefab name</param>
        /// <returns>The thumbnail image path, or empty string if not found</returns>
        private string GetThumbnail(string prefabName)
        {
            try
            {
                string[] prefabTypes = { "BuildingPrefab", "Prefab" };
                
                foreach (var prefabType in prefabTypes)
                {
                    var prefabId = new PrefabID(prefabType, prefabName);
                    if (_prefabSystem.TryGetPrefab(prefabId, out var prefabBase))
                    {
                        // Get thumbnail using ImageSystem like FindIt does
                        return ImageSystem.GetThumbnail(prefabBase) ?? string.Empty;
                    }
                }
                
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
