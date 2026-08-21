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

namespace ParkingFeeControl
{
    public static class ModLogger
    {
        public static void Info(string message)
        {
            Mod.Log?.Info(message);
        }

        public static void Warn(string message)
        {
            Mod.Log?.Warn(message);
        }

        public static void Error(string message)
        {
            Mod.Log?.Error(message);
        }

        public static void Debug(string message)
        {
            if (Mod.Settings?.DebugLogging == true)
            {
                Mod.Log?.Info($"{message}");
            }
        }

        public static void Debug(Func<string> messageFactory)
        {
            if (Mod.Settings?.DebugLogging == true)
            {
                Mod.Log?.Info(messageFactory());
            }
        }
    }
}
