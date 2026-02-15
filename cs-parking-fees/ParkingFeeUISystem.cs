using Colossal.UI.Binding;
using Game.UI;
using Game.UI.InGame;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Areas;
using Game.Common;
using Unity.Collections;
using Unity.Entities;

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
        private const string DistrictPrefabIconPath = "Media/Game/Policies/PaidParking.svg";
        private const string DistrictEntityKeyPrefix = "district:";

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
        private Dictionary<string, PrefabBase> _prefabByNameCache;
        private EntityQuery _districtQuery;
        private NameSystem _nameSystem;

        /// <summary>
        /// Maps Entity.Index to Entity for district fee lookups within the current session.
        /// Rebuilt on each UI refresh. Not persisted — entity handles are session-stable.
        /// </summary>
        private Dictionary<int, Entity> _districtEntityMap = new Dictionary<int, Entity>();

        protected override void OnCreate()
        {
            base.OnCreate();

            // Get required systems
            _prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            _prefabUISystem = World.GetOrCreateSystemManaged<PrefabUISystem>();
            _imageSystem = World.GetOrCreateSystemManaged<ImageSystem>();
            _policySystem = World.GetOrCreateSystemManaged<ParkingPolicyModifierSystem>();
            _nameSystem = World.GetOrCreateSystemManaged<NameSystem>();
            _districtQuery = GetEntityQuery(
                ComponentType.ReadOnly<District>()
            );

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

            ModLogger.Info("UI System initialized");
        }

        private void LoadConfigFromMod()
        {
            // Initialize _currentConfig with data from Mod.Config
            // This ensures the binding has a valid object to work with
            var config = Mod.Config;
            _currentConfig = new ParkingFeeUIData
            {
                categories = config.Categories.Select(c => {
                    List<ParkingFeeUIData.PrefabData> prefabList;
                    
                    if (string.Equals(c.Type, ParkingFeeConfig.DistrictsCategoryType, StringComparison.OrdinalIgnoreCase))
                    {
                        prefabList = BuildDistrictPrefabData(c);
                    }
                    else
                    {
                        prefabList = c.Prefabs.Select(p => new ParkingFeeUIData.PrefabData
                        {
                            name = p.Name,
                            displayName = GetDisplayName(p.Name),
                            thumbnail = GetThumbnail(p.Name),
                            fee = p.Fee ?? c.DefaultFee
                        }).ToList();
                    }

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

        /// <summary>
        /// Builds UI data for all districts. Fee is read from the DistrictParkingFee
        /// ECS component on each entity (persisted in the save file). Districts without
        /// the component use the category default fee.
        /// </summary>
        private List<ParkingFeeUIData.PrefabData> BuildDistrictPrefabData(ParkingFeeConfig.Category category)
        {
            var result = new List<ParkingFeeUIData.PrefabData>();
            _districtEntityMap.Clear();

            if (_districtQuery == null)
            {
                return result;
            }

            var districts = _districtQuery.ToEntityArray(Allocator.Temp);
            try
            {
                foreach (var district in districts)
                {
                    _districtEntityMap[district.Index] = district;

                    var displayName = GetDistrictDisplayName(district);
                    var entityKey = $"{DistrictEntityKeyPrefix}{district.Index}";

                    bool hasComponent = EntityManager.HasComponent<DistrictParkingFee>(district);
                    int fee = hasComponent
                        ? EntityManager.GetComponentData<DistrictParkingFee>(district).m_Fee
                        : category.DefaultFee;

                    ModLogger.Debug($"  [UI] District '{displayName}' (#{district.Index}): hasComponent={hasComponent}, fee=${fee}{(hasComponent ? "" : " (default)")}");

                    result.Add(new ParkingFeeUIData.PrefabData
                    {
                        name = entityKey,
                        displayName = displayName,
                        thumbnail = DistrictPrefabIconPath,
                        fee = fee
                    });
                }
            }
            finally
            {
                districts.Dispose();
            }

            return result;
        }

        private string GetDistrictDisplayName(Entity district)
        {
            try
            {
                if (_nameSystem != null)
                {
                    return _nameSystem.GetRenderedLabelName(district);
                }
            }
            catch (Exception ex)
            {
                ModLogger.Debug($"Failed to resolve district display name for entity #{district.Index}: {ex}");
            }

            return $"District #{district.Index}";
        }

        private void RefreshConfigFromMod()
        {
            try
            {
                Mod.ReloadConfig();
                _prefabByNameCache = null;
                LoadConfigFromMod();
                NotifyConfigChanged();

                ModLogger.Debug("UI config refreshed on panel open");
            }
            catch (Exception ex)
            {
                ModLogger.Warn($"Failed to refresh UI config: {ex.Message}");
            }
        }

        private void ApplyNow()
        {
            try
            {
                _policySystem?.ApplyNow(resetTimer: true);
                ModLogger.Debug("Apply now requested");
            }
            catch (Exception ex)
            {
                ModLogger.Warn($"Failed to apply fees immediately: {ex.Message}");
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
            if (category == null)
                return;

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

            // Districts: persist fees to ECS components on the entity (saved with the game)
            if (IsDistrictsCategory(update.categoryType))
            {
                foreach (var prefab in category.prefabs)
                {
                    if (TryParseDistrictEntityIndex(prefab.name, out int entityIndex))
                    {
                        SetDistrictFee(entityIndex, (int)Math.Round(prefab.fee));
                    }
                }

                // Save only the default fee to config (for newly created districts)
                var districtCategory = Mod.Config.Categories.FirstOrDefault(c =>
                    string.Equals(c.Type, ParkingFeeConfig.DistrictsCategoryType, StringComparison.OrdinalIgnoreCase));
                if (districtCategory != null)
                {
                    districtCategory.DefaultFee = (int)Math.Round(update.newFee);
                }
                Mod.Config.Save();
                return;
            }

            // Non-district categories: persist per-prefab fees to JSON config
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
        }

        private void UpdatePrefabFee(PrefabFeeUpdate update)
        {
            var category = _currentConfig.categories.FirstOrDefault(c => c.type == update.categoryType);
            if (category == null)
                return;

            var prefab = category.prefabs.FirstOrDefault(p => p.name == update.prefabName);
            if (prefab == null)
                return;

            prefab.fee = (float)Math.Round(update.newFee);
            NotifyConfigChanged();

            // Districts: persist fee to ECS component on the entity (saved with the game)
            if (IsDistrictsCategory(update.categoryType))
            {
                if (TryParseDistrictEntityIndex(update.prefabName, out int entityIndex))
                {
                    SetDistrictFee(entityIndex, (int)Math.Round(update.newFee));
                }
                return;
            }

            // Non-district: persist to JSON config
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
                var prefabBase = TryResolvePrefabByName(prefabName);
                if (prefabBase != null)
                {
                    _prefabUISystem.GetTitleAndDescription(_prefabSystem.GetEntity(prefabBase), out var titleId, out var _);

                    if (GameManager.instance.localizationManager.activeDictionary.TryGetValue(titleId, out var name)
                        && !string.IsNullOrWhiteSpace(name))
                    {
                        return name;
                    }

                    return ToFriendlyName(prefabBase.name);
                }

                return ToFriendlyName(prefabName);
            }
            catch (Exception ex)
            {
                ModLogger.Debug($"Failed to resolve localized display name for prefab '{prefabName}': {ex}");
                return ToFriendlyName(prefabName);
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
                var fallbackIcon = GetDefaultFallbackIcon();
                var prefabBase = TryResolvePrefabByName(prefabName);
                if (prefabBase != null)
                {
                    var thumbnail = ImageSystem.GetThumbnail(prefabBase);
                    if (!string.IsNullOrEmpty(thumbnail) && !IsPlaceholderThumbnail(thumbnail))
                    {
                        return thumbnail;
                    }

                    if (_imageSystem != null)
                    {
                        var entity = _prefabSystem.GetEntity(prefabBase);
                        var groupIcon = _imageSystem.GetGroupIcon(entity);
                        if (!string.IsNullOrEmpty(groupIcon))
                        {
                            return groupIcon;
                        }
                    }

                    return fallbackIcon;
                }
                return fallbackIcon;
            }
            catch (Exception ex)
            {
                ModLogger.Debug($"Failed to resolve thumbnail for prefab '{prefabName}': {ex}");
                return GetDefaultFallbackIcon();
            }
        }

        private static string GetDefaultFallbackIcon()
        {
            return "Media/Game/Icons/Parking.svg";
        }

        private static bool IsPlaceholderThumbnail(string value)
        {
            return string.Equals(value, "Media/Placeholder.svg", StringComparison.OrdinalIgnoreCase);
        }

        private PrefabBase TryResolvePrefabByName(string prefabName)
        {
            if (string.IsNullOrWhiteSpace(prefabName) || _prefabSystem == null)
            {
                return null;
            }

            EnsurePrefabCache();

            if (_prefabByNameCache != null && _prefabByNameCache.TryGetValue(prefabName, out var prefab))
            {
                return prefab;
            }

            if (_prefabByNameCache != null && _prefabByNameCache.Count == 0)
            {
                _prefabByNameCache = null;
                EnsurePrefabCache();

                if (_prefabByNameCache != null && _prefabByNameCache.TryGetValue(prefabName, out prefab))
                {
                    return prefab;
                }
            }

            return null;
        }

        private void EnsurePrefabCache()
        {
            if (_prefabByNameCache != null || _prefabSystem == null)
            {
                return;
            }

            _prefabByNameCache = new Dictionary<string, PrefabBase>(StringComparer.OrdinalIgnoreCase);

            try
            {
                IEnumerable<PrefabBase> prefabs = null;

                var prefabsProperty = typeof(PrefabSystem).GetProperty("prefabs", BindingFlags.Instance | BindingFlags.NonPublic);
                if (prefabsProperty?.GetValue(_prefabSystem) is IEnumerable<PrefabBase> propPrefabs)
                {
                    prefabs = propPrefabs;
                }
                else
                {
                    var prefabsField = typeof(PrefabSystem).GetField("m_Prefabs", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (prefabsField?.GetValue(_prefabSystem) is IEnumerable<PrefabBase> fieldPrefabs)
                    {
                        prefabs = fieldPrefabs;
                    }
                }

                if (prefabs == null)
                {
                    return;
                }

                foreach (var prefab in prefabs)
                {
                    if (prefab?.name == null)
                    {
                        continue;
                    }

                    if (!_prefabByNameCache.ContainsKey(prefab.name))
                    {
                        _prefabByNameCache.Add(prefab.name, prefab);
                    }
                }
            }
            catch (Exception ex)
            {
                ModLogger.Debug($"Failed to build prefab cache via reflection: {ex}");
            }
        }

        // ── District ECS helpers ──────────────────────────────────────────────

        private static bool IsDistrictsCategory(string categoryType)
        {
            return string.Equals(categoryType, ParkingFeeConfig.DistrictsCategoryType, StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParseDistrictEntityIndex(string key, out int entityIndex)
        {
            entityIndex = 0;
            if (string.IsNullOrEmpty(key) || !key.StartsWith(DistrictEntityKeyPrefix, StringComparison.OrdinalIgnoreCase))
                return false;
            return int.TryParse(key.Substring(DistrictEntityKeyPrefix.Length), out entityIndex);
        }

        /// <summary>
        /// Writes or updates the DistrictParkingFee component on a district entity.
        /// </summary>
        private void SetDistrictFee(int entityIndex, int fee)
        {
            if (!_districtEntityMap.TryGetValue(entityIndex, out var entity))
                return;

            string districtName = GetDistrictDisplayName(entity);
            var feeComponent = new DistrictParkingFee(fee);
            if (EntityManager.HasComponent<DistrictParkingFee>(entity))
            {
                EntityManager.SetComponentData(entity, feeComponent);
                ModLogger.Debug($"  District '{districtName}' (#{entityIndex}): updated fee to ${fee}");
            }
            else
            {
                EntityManager.AddComponentData(entity, feeComponent);
                ModLogger.Debug($"  District '{districtName}' (#{entityIndex}): added component with fee ${fee}");
            }
        }

        // ── Shared UI helpers ────────────────────────────────────────────────

        private static string ToFriendlyName(string prefabName)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                return prefabName ?? string.Empty;
            }

            var sb = new StringBuilder(prefabName.Length + 8);
            char prev = '\0';

            foreach (var ch in prefabName)
            {
                if (ch == '_' || ch == '-')
                {
                    if (sb.Length > 0 && sb[sb.Length - 1] != ' ')
                    {
                        sb.Append(' ');
                    }
                    prev = ch;
                    continue;
                }

                if (sb.Length > 0)
                {
                    var addSpace = (char.IsUpper(ch) && (char.IsLower(prev) || char.IsDigit(prev)))
                        || (char.IsDigit(ch) && !char.IsDigit(prev) && prev != ' ')
                        || (char.IsLetter(ch) && char.IsDigit(prev));

                    if (addSpace && sb[sb.Length - 1] != ' ')
                    {
                        sb.Append(' ');
                    }
                }

                sb.Append(ch);
                prev = ch;
            }

            return sb.ToString().Trim();
        }
    }
}
