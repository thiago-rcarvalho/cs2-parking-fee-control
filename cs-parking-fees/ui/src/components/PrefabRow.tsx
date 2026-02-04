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
