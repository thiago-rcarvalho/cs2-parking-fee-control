using System;
using System.IO;
using Colossal.IO.AssetDatabase;
using Game.Settings;
using Game.Modding;
using Newtonsoft.Json;

namespace ParkingFeeControl
{
    /// <summary>
    /// Settings for Parking Fee Control mod that are managed via in-game UI.
    /// These are NOT persisted to parking-config.json.
    /// </summary>
    [FileLocation("ModsSettings/ParkingFeeControl/Settings")]
    [SettingsUITabOrder("Settings")]
    [SettingsUIGroupOrder("General")]
    [SettingsUIShowGroupName("General")]
    public class ModSettings : ModSetting
    {
        public ModSettings(IMod mod) : base(mod)
        {
        }

        public override void SetDefaults()
        {
            Enabled = true;
            DebugLogging = false;
            UpdateFrequencyMinutes = UpdateFrequency.Minutes5;
            IgnoreTag = IgnoreTagType.Npf;
        }

        [SettingsUISection("Settings", "General")]
        public bool Enabled { get; set; } = true;

        [SettingsUISection("Settings", "General")]
        public bool DebugLogging { get; set; } = false;

        [SettingsUISection("Settings", "General")]
        public UpdateFrequency UpdateFrequencyMinutes { get; set; } = UpdateFrequency.Minutes5;

        [SettingsUISection("Settings", "General")]
        public IgnoreTagType IgnoreTag { get; set; } = IgnoreTagType.Npf;

        /// <summary>
        /// Get update frequency in seconds.
        /// </summary>
        public int GetUpdateFrequencySeconds()
        {
            return (int)UpdateFrequencyMinutes * 60;
        }

        /// <summary>
        /// Check if building should be ignored based on custom name.
        /// </summary>
        public bool ShouldIgnoreByName(string customName)
        {
            if (string.IsNullOrEmpty(customName))
                return false;

            string tag = GetIgnoreTagString();
            if (string.IsNullOrEmpty(tag))
                return false;

            return customName.IndexOf(tag, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Get the ignore tag as string.
        /// </summary>
        public string GetIgnoreTagString()
        {
            return IgnoreTag switch
            {
                IgnoreTagType.Npf => "[npf]",
                IgnoreTagType.Nofee => "[nofee]",
                IgnoreTagType.PipeNpf => "|npf|",
                _ => "[npf]"
            };
        }

        /// <summary>
        /// Update frequency enum.
        /// </summary>
        public enum UpdateFrequency
        {
            Minutes1 = 1,
            Minutes3 = 3,
            Minutes5 = 5,
            Minutes10 = 10,
            Minutes15 = 15,
            Minutes30 = 30,
            Minutes60 = 60
        }

        /// <summary>
        /// Ignore tag type enum.
        /// </summary>
        public enum IgnoreTagType
        {
            Npf,
            Nofee,
            PipeNpf
        }
    }
}