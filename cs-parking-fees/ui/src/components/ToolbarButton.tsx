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
import { getModule } from 'cs2/modding';

// Custom floating button styled like the game's native buttons
// Using longhand CSS properties as cohtml doesn't support shorthand with variables
const buttonStyle: React.CSSProperties = {
  marginRight: '6rem',
  marginBottom: '6rem',
};

const iconStyle: React.CSSProperties = {
  width: '100%',
  height: '100%',
};

// Parking icon using the game's default asset
const ParkingIcon: React.FC = () => (
  <img
    src="Media/Game/Icons/Parking.svg"
    alt="Parking Icon"
    style={iconStyle}
    draggable={false}
  />
);

interface ToolbarButtonProps {
  onClick: () => void;
}

export const ToolbarButton: React.FC<ToolbarButtonProps> = ({ onClick }) => {
  const floatingButtonClasses = getModule(
    'game-ui/common/input/button/floating-icon-button.module.scss',
    'classes'
  ) as Record<string, string> | undefined;

  const handleClick = () => {
    //console.log('[ParkingFeeControl] Button clicked!');
    onClick();
  };

  return (
    <button
      type="button"
      className={floatingButtonClasses?.button}
      style={buttonStyle}
      onClick={handleClick}
      title="Parking Fee Configuration"
    >
      <ParkingIcon />
    </button>
  );
};
