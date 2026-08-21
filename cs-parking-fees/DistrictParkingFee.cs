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

using Colossal.Serialization.Entities;
using Unity.Entities;

namespace ParkingFeeControl
{
    /// <summary>
    /// ECS component that stores the parking fee for a district.
    /// Attached directly to district entities and serialized with the save file
    /// via the game's built-in ISerializable framework.
    /// Entity references are automatically remapped on save/load, so the fee
    /// persists correctly across sessions without relying on district names.
    /// Safe to remove the mod: the game silently ignores unknown components on load.
    /// </summary>
    public struct DistrictParkingFee : IComponentData, IQueryTypeParameter, ISerializable
    {
        public int m_Fee;

        public DistrictParkingFee(int fee)
        {
            m_Fee = fee;
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(m_Fee);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out m_Fee);
        }
    }
}
