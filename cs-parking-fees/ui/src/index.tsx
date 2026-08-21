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

import React from 'react';
import { ModRegistrar } from 'cs2/modding';
import { ParkingFeeApp } from './components/ParkingFeeApp';
import { VanillaComponentResolver } from './mods/VanillaComponentResolver';

const register: ModRegistrar = (moduleRegistry) => {
  // Initialize the VanillaComponentResolver with the registry
  VanillaComponentResolver.setRegistry(moduleRegistry);
  
  // Register the component that contains both the button and the panel
  moduleRegistry.append('GameTopLeft', ParkingFeeApp);

};

export default register;
