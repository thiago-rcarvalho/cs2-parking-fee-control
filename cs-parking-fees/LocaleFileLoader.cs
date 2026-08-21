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
    /// so translators can edit plain JSON files, and exposes them to the
    /// game's localization manager via <see cref="FileDictionarySource"/>.
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

        /// <summary>
        /// Adapts an in-memory locale dictionary to the game's <see cref="IDictionarySource"/>
        /// contract. The entries are already loaded and validated by <see cref="LoadDictionaries"/>,
        /// so this class has nothing to do beyond exposing them and has no resources to release.
        /// </summary>
        public sealed class FileDictionarySource : IDictionarySource
        {
            private readonly IReadOnlyDictionary<string, string> _entries;

            public FileDictionarySource(Dictionary<string, string> entries)
            {
                _entries = entries ?? new Dictionary<string, string>();
            }

            public IEnumerable<KeyValuePair<string, string>> ReadEntries(
                IList<IDictionaryEntryError> errors,
                Dictionary<string, int> indexCounts)
            {
                foreach (var entry in _entries)
                {
                    yield return entry;
                }
            }

            public void Unload()
            {
                // Entries live in managed memory only; nothing to release.
            }
        }
    }
}
