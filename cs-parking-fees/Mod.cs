using Colossal.Logging;
using Colossal.IO.AssetDatabase;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Unity.Entities;
using System.Linq;
using System.Reflection;

namespace ParkingFeeControl
{
    /// <summary>
    /// Main mod class for Parking Fee Control mod.
    /// Allows customization of parking fees throughout the city.
    /// </summary>
    public class Mod : IMod
    {
        public static string ModVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        /// <summary>
        /// Logger instance for the mod.
        /// </summary>
        public static ILog Log { get; private set; } = null!;

        /// <summary>
        /// Path to the mod's installation directory.
        /// </summary>
        public static string ModPath { get; private set; } = string.Empty;

        /// <summary>
        /// Path to the configuration directory.
        /// </summary>
        public static string ConfigDirectory => ModPath;

        /// <summary>
        /// Current fee configuration settings (persisted to parking-config.json).
        /// </summary>
        public static ParkingFeeConfig Config { get; private set; } = new ParkingFeeConfig();

        /// <summary>
        /// Current mod settings (managed via in-game UI).
        /// </summary>
        public static ModSettings Settings { get; private set; } = null!;

        /// <summary>
        /// Called when the mod is loaded by the game.
        /// </summary>
        public void OnLoad(UpdateSystem updateSystem)
        {
            // Initialize logger with visible errors for debugging
            Log = LogManager.GetLogger(nameof(ParkingFeeControl)).SetShowsErrorsInUI(true);
            ModLogger.Info("============================================");
            ModLogger.Info("Parking Fee Control mod loading...");
            ModLogger.Info($"Mod version: {ModVersion}");
            try
            {
                ModLogger.Info($"Game version: {Game.Version.current.fullVersion}");
            }
            catch (System.Exception ex)
            {
                ModLogger.Debug($"Could not read game version during load: {ex}");
            }
            try
            {
                var localeId = GameManager.instance?.localizationManager?.activeLocaleId;
                if (!string.IsNullOrEmpty(localeId))
                    ModLogger.Info($"Game language: {localeId}");
            }
            catch (System.Exception ex)
            {
                ModLogger.Debug($"Could not read active locale during load: {ex}");
            }
            ModLogger.Info("============================================");

            // Get mod path from GameManager
            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            {
                ModPath = System.IO.Path.GetDirectoryName(asset.path) ?? string.Empty;
                ModLogger.Info($"Mod path: {ModPath}");
            }
            else
            {
                ModLogger.Warn("Could not determine mod path!");
            }

            try
            {
                // Load mod settings (UI-managed settings)
                Settings = new ModSettings(this);
                Settings.RegisterInOptionsUI();
                AssetDatabase.global.LoadSettings(nameof(ParkingFeeControl), Settings, new ModSettings(this));

                // Load fee configuration (JSON-persisted fees)
                Config = ParkingFeeConfig.Load();
                Config.LogSettings();

                // Register localization from external JSON files if present, otherwise fall back to embedded LocaleEN
                var locales = ParkingFeeControl.LocaleFileLoader.LoadDictionaries(ModPath);
                if (locales != null)
                {
                    if (locales.Count > 0)
                    {
                        ModLogger.Debug(() => $"Locale files found: {string.Join(", ", locales.Keys)}");
                    }

                    foreach (var kv in locales)
                    {
                        GameManager.instance.localizationManager.AddSource(kv.Key, new ParkingFeeControl.LocaleFileLoader.FileDictionarySource(kv.Value));
                    }
                }

                // Register parking policy modifier system
                updateSystem.UpdateAt<ParkingPolicyModifierSystem>(SystemUpdatePhase.GameSimulation);
                
                // Register UI system
                updateSystem.UpdateAt<UI.ParkingFeeUISystem>(SystemUpdatePhase.UIUpdate);

                ModLogger.Info("✓ Systems registered successfully!");
                ModLogger.Info("✓ Parking Fee Control mod loaded successfully!");
                foreach (var cat in Config.Categories)
                {
                    ModLogger.Info($"✓ Default {cat.Type} parking fee set to: ${cat.DefaultFee}");
                }
                ModLogger.Info("============================================");
            }
            catch (System.Exception ex)
            {
                ModLogger.Error($"✗ Failed to load Parking Fee Control mod: {ex}");
            }
        }

        /// <summary>
        /// Called when the mod is being unloaded/disposed.
        /// </summary>
        public void OnDispose()
        {
            if (Log != null)
            {
                ModLogger.Info("============================================");
                ModLogger.Info("Parking Fee Control mod unloading...");
                ModLogger.Info("============================================");
            }
        }

        /// <summary>
        /// Reload configuration from disk (rescans mods).
        /// </summary>
        public static void ReloadConfig()
        {
            Config = ParkingFeeConfig.Load(showLog: true);
            ModLogger.Info("Configuration reloaded successfully");
        }
    }
}
