import React, { useState, useEffect } from 'react';
import { PrefabRow } from './PrefabRow';
import { FeeSlider } from './FeeSlider';
import { Category } from '../types/parking-types';
import { getIconPath } from '../utils/icon';
// Simple icon loading: UI will prepend Media/Game/Icons/ to an icon name
// unless a path is provided. We attempt to load it and fall back to default.

const categoryRowStyle: React.CSSProperties = {
  marginBottom: '10rem',
  backgroundColor: 'rgba(255, 255, 255, 0.05)',
  borderTopLeftRadius: '4rem',
  borderTopRightRadius: '4rem',
  borderBottomLeftRadius: '4rem',
  borderBottomRightRadius: '4rem',
  overflow: 'hidden',
};

const categoryHeaderStyle: React.CSSProperties = {
  display: 'flex',
  alignItems: 'center',
  paddingTop: '10rem',
  paddingRight: '10rem',
  paddingBottom: '10rem',
  paddingLeft: '10rem',
  gap: '10rem',
};

const expandButtonStyle: React.CSSProperties = {
  backgroundColor: 'transparent',
  border: 'none',
  color: 'white',
  cursor: 'pointer',
  fontSize: '12rem',
  width: '24rem',
};

const categoryIconStyle: React.CSSProperties = {
  fontSize: '20rem',
};

const categoryNameStyle: React.CSSProperties = {
  flex: 1,
  fontWeight: 'bold',
    textTransform: 'capitalize',
};

const feeDisplayStyle: React.CSSProperties = {
  color: 'var(--accentColorNormal)',
  fontWeight: 'bold',
  minWidth: '70rem',
  textAlign: 'right',
};

const prefabListStyle: React.CSSProperties = {
  paddingTop: '0',
  paddingRight: '10rem',
  paddingBottom: '10rem',
  paddingLeft: '40rem',
};

interface CategoryRowProps {
  category: Category;
  onCategoryFeeChange: (newFee: number) => void;
  onPrefabFeeChange: (prefabName: string, newFee: number) => void;
}

export const CategoryRow: React.FC<CategoryRowProps> = ({
  category,
  onCategoryFeeChange,
  onPrefabFeeChange
}) => {
  const [expanded, setExpanded] = useState(false);
  const [iconSrc, setIconSrc] = useState<string | null>(null);
  
  const defaultIconPath = 'Media/Game/Icons/Parking.svg';

  const getDisplayType = (type: string): string => {
    if (!type) return type;
    return type.charAt(0).toUpperCase() + type.slice(1).toLowerCase();
  };


  // Calculate the range of prefab fees
  const fees = category.prefabs.map(p => p.fee);
  const minFee = fees.length > 0 ? Math.min(...fees) : 0;
  const maxFee = fees.length > 0 ? Math.max(...fees) : 0;
  const feeRange = minFee === maxFee ? `$${minFee}` : `$${minFee}-$${maxFee}`;

  // Usar o label do FeeSlider para exibir o range dos itens
  const sliderStyle: React.CSSProperties = {
    minWidth: '120rem',
    maxWidth: '160rem',
  };

  useEffect(() => {
    // Use shared helper to build icon path (always {name}.svg)
    let mounted = true;
    const path = getIconPath(category.icon || category.type);

    const img = new Image();
    img.onload = () => { if (mounted) setIconSrc(path); };
    img.onerror = () => { if (mounted) setIconSrc(defaultIconPath); };
    img.src = path;

    return () => { mounted = false; img.onload = null; img.onerror = null; };
  }, [category.icon, category.type]);

  return (
    <div style={categoryRowStyle}>
      <div style={categoryHeaderStyle}>
        <button 
          style={expandButtonStyle}
          onClick={() => setExpanded(!expanded)}
        >
          {expanded ? '▼' : '▶'}
        </button>
        <div style={categoryIconStyle}>{iconSrc ? <img src={iconSrc} style={{width:'20rem',height:'20rem'}} onError={(e) => { (e.currentTarget as HTMLImageElement).src = defaultIconPath; }} /> : <img src={defaultIconPath} style={{width:'20rem',height:'20rem'}} onError={(e) => { (e.currentTarget as HTMLImageElement).src = defaultIconPath; }} />}</div>
        <div style={categoryNameStyle}>{getDisplayType(category.type)}</div>
        <div style={{display: 'flex', alignItems: 'center', gap: '4rem'}}>
          <FeeSlider
            label={feeRange}
            value={category.defaultFee}
            min={0}
            max={50}
            onChange={onCategoryFeeChange}
            // @ts-ignore
            style={sliderStyle}
          />
          <div style={feeDisplayStyle}>${category.defaultFee}</div>
        </div>
      </div>
      {expanded && (
        <div style={prefabListStyle}>
          {category.prefabs.map(prefab => (
            <PrefabRow
              key={prefab.name}
              prefab={prefab}
              onFeeChange={(newFee) => onPrefabFeeChange(prefab.name, newFee)}
            />
          ))}
        </div>
      )}
    </div>
  );
};
