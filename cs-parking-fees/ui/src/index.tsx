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
