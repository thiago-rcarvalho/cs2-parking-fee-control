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

const sliderContainerStyle: React.CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: '4rem',
  minWidth: '120rem',
};

const sliderLabelStyle: React.CSSProperties = {
  display: 'flex',
  justifyContent: 'space-between',
  fontSize: '11rem',
  color: 'rgba(255, 255, 255, 0.7)',
};

const sliderStyle: React.CSSProperties = {
  width: '100%',
  height: '6rem',
  cursor: 'pointer',
};

interface FeeSliderProps {
  label: string;
  value: number;
  min: number;
  max: number;
  onChange: (value: number) => void;
  disabled?: boolean;
}

export const FeeSlider: React.FC<FeeSliderProps> = ({
  label,
  value,
  min,
  max,
  onChange,
  disabled = false
}) => {
  return (
    <div style={sliderContainerStyle}>
      {label && (
        <div style={sliderLabelStyle}>
          <span>{label}</span>
          <span>${value}</span>
        </div>
      )}
      <input
        type="range"
        min={min}
        max={max}
        value={value}
        onChange={(e) => onChange(parseInt(e.target.value))}
        disabled={disabled}
        style={sliderStyle}
      />
    </div>
  );
};
