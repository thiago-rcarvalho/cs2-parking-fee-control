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

export interface ParkingFeeConfig {
  enabled: boolean;
  defaultParkingFee: number;
  categories: Category[];
}

export interface Category {
  type: string;
  defaultFee: number;
  icon?: string;
  prefabs: Prefab[];
}

export interface Prefab {
  name: string;
  displayName: string;
  thumbnail?: string;
  fee: number;
}

export interface CategoryFeeUpdate {
  categoryType: string;
  newFee: number;
}

export interface PrefabFeeUpdate {
  categoryType: string;
  prefabName: string;
  newFee: number;
}
