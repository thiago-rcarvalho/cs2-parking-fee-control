// This file is part of Parking Fee Control mod.
// Copyright (C) 2026 thiago-rcarvalho
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// SPDX-License-Identifier: GPL-3.0-or-later

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
    [SettingsUIGroupOrder("General", "About")]
    [SettingsUIShowGroupName("General", "About")]
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

        [SettingsUISection("Settings", "About")]
        public string Version => Mod.ModVersion;

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