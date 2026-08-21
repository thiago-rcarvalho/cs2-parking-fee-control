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
import { FeeSlider } from './FeeSlider';
import { Prefab } from '../types/parking-types';

const prefabRowStyle: React.CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  paddingTop: '5rem',
  paddingRight: '0',
  paddingBottom: '5rem',
  paddingLeft: '0',
  gap: '10rem',
};

const prefabNameStyle: React.CSSProperties = {
  flex: 1,
  color: 'rgba(255, 255, 255, 0.8)',
  fontSize: '12rem',
};

const feeDisplayStyle: React.CSSProperties = {
  color: 'var(--accentColorNormal)',
  fontWeight: 'bold',
  minWidth: '50rem',
  textAlign: 'right',
};

interface PrefabRowProps {
  prefab: Prefab;
  onFeeChange: (newFee: number) => void;
}

export const PrefabRow: React.FC<PrefabRowProps> = ({ prefab, onFeeChange }) => {
  const displayName = prefab.displayName && prefab.displayName.trim() !== '' ? prefab.displayName : prefab.name;
  
  return (
    <div style={prefabRowStyle}>
      <div style={prefabNameStyle}>{displayName}</div>
      <FeeSlider
        label=""
        value={prefab.fee}
        min={0}
        max={50}
        onChange={onFeeChange}
      />
      <div style={feeDisplayStyle}>${prefab.fee}</div>
    </div>
  );
};
